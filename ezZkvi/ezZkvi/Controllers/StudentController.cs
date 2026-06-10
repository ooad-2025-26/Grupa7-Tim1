using ezZkvi.Data;
using ezZkvi.Models;
using ezZkvi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const int SistemskiBrojPitanja = 10;
        private const int VremenskoOgranicenjeMinuta = 7;

        private async Task<List<SelectListItem>> GetPredmetiZaSimulacijuAsync(int? selectedPredmetId = null)
        {
            return await _context.Predmet
                .OrderBy(p => p.Naziv)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Naziv,
                    Selected = selectedPredmetId.HasValue && p.Id == selectedPredmetId.Value
                })
                .ToListAsync();
        }

        private async Task<SimulacijaKvizaViewModel> BuildSimulationAsync(int predmetId)
        {
            var predmet = await _context.Predmet.FindAsync(predmetId);


            var model = new SimulacijaKvizaViewModel
            {
                PredmetId = predmetId,
                PredmetNaziv = predmet?.Naziv ?? "Odabrani predmet",
                BrojPitanja = SistemskiBrojPitanja,
                VremenskoOgranicenjeMinuta = VremenskoOgranicenjeMinuta,
                StartedAtUtcTicks = DateTime.UtcNow.Ticks
            };

            if (predmet == null)
            {
                model.ErrorMessage = "Odabrani predmet ne postoji.";
                return model;
            }

            var svaPitanja = await _context.Pitanje
                .Where(p => p.PredmetId == predmetId)
                .ToListAsync();

            var pitanjeIds = svaPitanja.Select(p => p.Id).ToList();

            var sviOdgovori = await _context.Odgovor
                .Where(o => pitanjeIds.Contains(o.PitanjeId))
                .ToListAsync();

            var odgovoriPoPitanju = sviOdgovori
                .GroupBy(o => o.PitanjeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var validnaPitanja = svaPitanja
                .Where(p => odgovoriPoPitanju.ContainsKey(p.Id)
                            && odgovoriPoPitanju[p.Id].Count >= 2
                            && odgovoriPoPitanju[p.Id].Count(o => o.IsTacan) == 1)
                .ToList();

            if (validnaPitanja.Count == 0)
            {
                model.ErrorMessage = "Za odabrani predmet nema validnih pitanja. Svako pitanje mora imati najmanje dva odgovora i tačno jedan tačan odgovor.";
                return model;
            }

            var odabranaPitanja = SelectQuestionsByDifficulty(validnaPitanja, SistemskiBrojPitanja);

            model.BrojPitanja = odabranaPitanja.Count;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var sesija = new KvizSesija
            {
                TraziBrojPitanja = odabranaPitanja.Count,
                VremenskoOgranicenje = VremenskoOgranicenjeMinuta,
                Status = StatusSesije.U_TOKU,
                StudentId = userId,
                PredmetId = predmetId,
                BrojTacnih = 0,
                Procenat = 0,
                DatumPocetka = now,
                DatumZavrsetka = now
            };

            _context.KvizSesije.Add(sesija);
            await _context.SaveChangesAsync();

            _context.KvizSesijaPitanja.AddRange(
                odabranaPitanja.Select((p, index) => new KvizSesijaPitanje
                {
                    KvizSesijaId = sesija.ID,
                    PitanjeId = p.Id,
                    RedniBroj = index + 1,
                    BrojBodova = 0,
                    Tacno = 0
                })
            );

            await _context.SaveChangesAsync();

            model.KvizSesijaId = sesija.ID;
            model.PreostaloSekundi = VremenskoOgranicenjeMinuta * 60;
            model.PocetniIndex = 0;

            model.Questions = odabranaPitanja
                .Select(p => new SimulacijaPitanjeViewModel
                {
                    Id = p.Id,
                    TekstPitanja = p.TekstPitanja,
                    Tezina = p.Tezina,
                    Odgovori = Shuffle(odgovoriPoPitanju[p.Id])
                        .Select(o => new SimulacijaOdgovorViewModel
                        {
                            Id = o.Id,
                            Tekst = o.Tekst
                        })
                        .ToList()
                })
                .ToList();

            return model;
        }

        private static List<Pitanje> SelectQuestionsByDifficulty(List<Pitanje> svaPitanja, int maxBrojPitanja)
        {
            var quotas = new Dictionary<Tezina, int>
            {
                [Tezina.LAKO] = 5,
                [Tezina.SREDNJE] = 3,
                [Tezina.TESKO] = 2
            };

            var selected = new List<Pitanje>();

            foreach (var quota in quotas)
            {
                selected.AddRange(
                    Shuffle(svaPitanja.Where(p => p.Tezina == quota.Key))
                        .Take(quota.Value)
                );
            }

            if (selected.Count < maxBrojPitanja)
            {
                var selectedIds = selected.Select(p => p.Id).ToHashSet();

                var remaining = Shuffle(svaPitanja.Where(p => !selectedIds.Contains(p.Id)))
                    .Take(maxBrojPitanja - selected.Count);

                selected.AddRange(remaining);
            }

            return Shuffle(selected).Take(maxBrojPitanja).ToList();
        }

        private static List<T> Shuffle<T>(IEnumerable<T> source)
        {
            return source
                .OrderBy(_ => Random.Shared.Next())
                .ToList();
        }

        private static SimulacijaRezultatViewModel CalculateResult(
            SimulacijaSubmitViewModel submitModel,
            List<Pitanje> pitanja,
            List<Odgovor> odgovori)
        {
            var pitanjaPoId = pitanja.ToDictionary(p => p.Id);

            var odgovoriPoId = odgovori.ToDictionary(o => o.Id);

            var odgovoriPoPitanju = odgovori
                .GroupBy(o => o.PitanjeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var pregled = new List<SimulacijaPregledOdgovoraViewModel>();

            var tacno = 0;
            var netacno = 0;
            var neodgovoreno = 0;

            foreach (var korisnickiOdgovor in submitModel.Odgovori)
            {
                if (!pitanjaPoId.TryGetValue(korisnickiOdgovor.PitanjeId, out var pitanje))
                {
                    continue;
                }

                odgovoriPoPitanju.TryGetValue(pitanje.Id, out var ponudjeniOdgovori);
                ponudjeniOdgovori ??= new List<Odgovor>();

                var tacanOdgovor = ponudjeniOdgovori.FirstOrDefault(o => o.IsTacan);

                Odgovor? odabraniOdgovor = null;

                if (korisnickiOdgovor.OdgovorId.HasValue)
                {
                    odgovoriPoId.TryGetValue(korisnickiOdgovor.OdgovorId.Value, out odabraniOdgovor);

                    if (odabraniOdgovor?.PitanjeId != pitanje.Id)
                    {
                        odabraniOdgovor = null;
                    }
                }

                var jeOdgovoreno = odabraniOdgovor != null;
                var jeTacno = jeOdgovoreno && odabraniOdgovor!.IsTacan;

                if (jeTacno)
                {
                    tacno++;
                }
                else if (jeOdgovoreno)
                {
                    netacno++;
                }
                else
                {
                    neodgovoreno++;
                }

                pregled.Add(new SimulacijaPregledOdgovoraViewModel
                {
                    TekstPitanja = pitanje.TekstPitanja,
                    KorisnickiOdgovor = odabraniOdgovor?.Tekst,
                    TacanOdgovor = tacanOdgovor?.Tekst ?? "Nije definisan tačan odgovor",
                    JeTacno = jeTacno,
                    JeOdgovoreno = jeOdgovoreno
                });
            }

            var ukupno = pregled.Count;

            var procenat = ukupno == 0
                ? 0
                : (int)Math.Round((double)tacno / ukupno * 100);

            var elapsedSeconds = CalculateElapsedSeconds(
                submitModel.StartedAtUtcTicks,
                submitModel.TotalSeconds
            );

            return new SimulacijaRezultatViewModel
            {
                UkupnoPitanja = ukupno,
                TacnihOdgovora = tacno,
                NetacnihOdgovora = netacno,
                Neodgovorenih = neodgovoreno,
                Procenat = procenat,
                UtrosenoSekundi = elapsedSeconds,
                Pregled = pregled
            };
        }
        private static int CalculateElapsedSeconds(long startedAtUtcTicks, int totalSeconds)
        {
            if (startedAtUtcTicks <= 0)
            {
                return 0;
            }

            var startedAt = new DateTime(startedAtUtcTicks, DateTimeKind.Utc);
            var elapsed = (int)Math.Round((DateTime.UtcNow - startedAt).TotalSeconds);

            if (elapsed < 0)
            {
                return 0;
            }

            if (totalSeconds > 0 && elapsed > totalSeconds)
            {
                return totalSeconds;
            }

            return elapsed;
        }

        private async Task SaveSimulationResultAsync(SimulacijaRezultatViewModel rezultat, int predmetId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var student = await _context.Student.FirstOrDefaultAsync(s => s.Id == userId);

                if (student != null)
                {
                    student.BrojOdgovorenihPitanja += rezultat.UkupnoPitanja;
                    student.BrojTacnihOdgovora += rezultat.TacnihOdgovora;
                }
            }

            _context.KvizSesije.Add(new KvizSesija
            {
                TraziBrojPitanja = rezultat.UkupnoPitanja,
                VremenskoOgranicenje = VremenskoOgranicenjeMinuta,
                Status = StatusSesije.ZAVRSEN,
                StudentId = userId,
                PredmetId = predmetId,
                BrojTacnih = rezultat.TacnihOdgovora,
                Procenat = rezultat.Procenat,
                DatumZavrsetka = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
        private async Task<SimulacijaKvizaViewModel> BuildActiveSimulationModelAsync(int kvizSesijaId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sesija = await _context.KvizSesije
                .Include(s => s.Predmet)
                .FirstOrDefaultAsync(s =>
                    s.ID == kvizSesijaId &&
                    s.StudentId == userId &&
                    s.Status == StatusSesije.U_TOKU);

            if (sesija == null)
            {
                return new SimulacijaKvizaViewModel
                {
                    ErrorMessage = "Aktivna simulacija nije pronađena.",
                    Predmeti = await GetPredmetiZaSimulacijuAsync()
                };
            }

            var deadline = sesija.DatumPocetka.AddMinutes(sesija.VremenskoOgranicenje);

            if (DateTime.UtcNow >= deadline)
            {
                var rezultat = await FinishSimulationAndBuildResultAsync(sesija.ID);

                return new SimulacijaKvizaViewModel
                {
                    KvizSesijaId = sesija.ID,
                    PredmetId = sesija.PredmetId,
                    PredmetNaziv = sesija.Predmet?.Naziv ?? "Odabrani predmet",
                    BrojPitanja = rezultat?.UkupnoPitanja ?? 0,
                    VremenskoOgranicenjeMinuta = sesija.VremenskoOgranicenje,
                    Result = rezultat,
                    Predmeti = await GetPredmetiZaSimulacijuAsync(sesija.PredmetId)
                };
            }

            var stavke = await _context.KvizSesijaPitanja
                .Include(x => x.Pitanje)
                .Where(x => x.KvizSesijaId == sesija.ID)
                .OrderBy(x => x.RedniBroj)
                .ToListAsync();

            var pitanjeIds = stavke.Select(x => x.PitanjeId).ToList();

            var odgovori = await _context.Odgovor
                .Where(o => pitanjeIds.Contains(o.PitanjeId))
                .ToListAsync();

            var odgovoriPoPitanju = odgovori
                .GroupBy(o => o.PitanjeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var pocetniIndex = stavke.FindIndex(x => !x.OdgovorId.HasValue);

            if (pocetniIndex < 0)
            {
                pocetniIndex = stavke.Count - 1;
            }

            return new SimulacijaKvizaViewModel
            {
                KvizSesijaId = sesija.ID,
                PredmetId = sesija.PredmetId,
                PredmetNaziv = sesija.Predmet?.Naziv ?? "Odabrani predmet",
                BrojPitanja = stavke.Count,
                VremenskoOgranicenjeMinuta = sesija.VremenskoOgranicenje,
                PreostaloSekundi = (int)(deadline - DateTime.UtcNow).TotalSeconds,
                PocetniIndex = pocetniIndex,
                Predmeti = await GetPredmetiZaSimulacijuAsync(sesija.PredmetId),

                Questions = stavke.Select(x => new SimulacijaPitanjeViewModel
                {
                    Id = x.PitanjeId,
                    TekstPitanja = x.Pitanje?.TekstPitanja ?? "Nepoznato pitanje",
                    Tezina = x.Pitanje?.Tezina ?? Tezina.LAKO,
                    OdabraniOdgovorId = x.OdgovorId,
                    JeOdgovoreno = x.OdgovorId.HasValue,
                    Odgovori = odgovoriPoPitanju[x.PitanjeId]
                        .Select(o => new SimulacijaOdgovorViewModel
                        {
                            Id = o.Id,
                            Tekst = o.Tekst
                        })
                        .ToList()
                }).ToList()
            };
        }

        private async Task<SimulacijaRezultatViewModel?> FinishSimulationAndBuildResultAsync(int kvizSesijaId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sesija = await _context.KvizSesije
                .FirstOrDefaultAsync(s => s.ID == kvizSesijaId && s.StudentId == userId);

            if (sesija == null)
            {
                return null;
            }

            var vecObradjena = sesija.Status == StatusSesije.ZAVRSEN || sesija.Status == StatusSesije.ISTEKAO;

            var deadline = sesija.DatumPocetka.AddMinutes(sesija.VremenskoOgranicenje);
            var vrijemeIsteklo = DateTime.UtcNow >= deadline;

            var stavke = await _context.KvizSesijaPitanja
                .Include(x => x.Pitanje)
                .Include(x => x.Odgovor)
                .Where(x => x.KvizSesijaId == kvizSesijaId)
                .OrderBy(x => x.RedniBroj)
                .ToListAsync();

            if (stavke.Count == 0)
            {
                return null;
            }

            var pitanjeIds = stavke.Select(x => x.PitanjeId).ToList();

            var sviOdgovori = await _context.Odgovor
                .Where(o => pitanjeIds.Contains(o.PitanjeId))
                .ToListAsync();

            var odgovoriPoPitanju = sviOdgovori
                .GroupBy(o => o.PitanjeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var pregled = new List<SimulacijaPregledOdgovoraViewModel>();

            foreach (var stavka in stavke)
            {
                odgovoriPoPitanju.TryGetValue(stavka.PitanjeId, out var ponudjeniOdgovori);
                ponudjeniOdgovori ??= new List<Odgovor>();

                var tacanOdgovor = ponudjeniOdgovori.FirstOrDefault(o => o.IsTacan);

                pregled.Add(new SimulacijaPregledOdgovoraViewModel
                {
                    TekstPitanja = stavka.Pitanje?.TekstPitanja ?? "Nepoznato pitanje",
                    KorisnickiOdgovor = stavka.Odgovor?.Tekst,
                    TacanOdgovor = tacanOdgovor?.Tekst ?? "Nije definisan tačan odgovor",
                    JeTacno = stavka.Tacno == 1,
                    JeOdgovoreno = stavka.OdgovorId.HasValue
                });
            }

            var ukupno = stavke.Count;
            var tacno = stavke.Count(x => x.Tacno == 1);
            var neodgovoreno = stavke.Count(x => !x.OdgovorId.HasValue);
            var netacno = ukupno - tacno;

            var procenat = ukupno == 0
                ? 0
                : (int)Math.Round((double)tacno / ukupno * 100);

            var rezultat = new SimulacijaRezultatViewModel
            {
                UkupnoPitanja = ukupno,
                TacnihOdgovora = tacno,
                NetacnihOdgovora = netacno,
                Neodgovorenih = neodgovoreno,
                Procenat = procenat,
                UtrosenoSekundi = CalculateElapsedSeconds(
                    sesija.DatumPocetka.Ticks,
                    sesija.VremenskoOgranicenje * 60
                ),
                Pregled = pregled
            };

            if (!vecObradjena)
            {
                sesija.Status = vrijemeIsteklo
                    ? StatusSesije.ISTEKAO
                    : StatusSesije.ZAVRSEN;

                sesija.BrojTacnih = tacno;
                sesija.Procenat = procenat;
                sesija.DatumZavrsetka = DateTime.UtcNow;

                if (sesija.Status == StatusSesije.ZAVRSEN)
                {
                    var student = await _context.Student.FirstOrDefaultAsync(s => s.Id == userId);

                    if (student != null)
                    {
                        student.BrojOdgovorenihPitanja += ukupno;
                        student.BrojTacnihOdgovora += tacno;
                    }
                }

                await _context.SaveChangesAsync();
            }

            return rezultat;
        }

        private bool StudentExists(string id)
        {
            return _context.Student.Any(e => e.Id == id);
        }

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Student/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var model = new DashboardViewModel();

            var aktivnaSesija = await _context.KvizSesije
                                                .Include(s => s.Predmet)
                                                .Where(s => s.StudentId == userId && s.Status == StatusSesije.U_TOKU)
                                                .OrderByDescending(s => s.DatumPocetka)
                                                .FirstOrDefaultAsync();

            if (aktivnaSesija != null)
            {
                var deadline = aktivnaSesija.DatumPocetka.AddMinutes(aktivnaSesija.VremenskoOgranicenje);

                if (DateTime.UtcNow >= deadline)
                {
                    await FinishSimulationAndBuildResultAsync(aktivnaSesija.ID);
                }
                else
                {
                    model.AktivnaSesijaId = aktivnaSesija.ID;
                    model.AktivnaSesijaPredmetNaziv = aktivnaSesija.Predmet?.Naziv ?? "Odabrani predmet";
                    model.AktivnaSesijaPreostaloSekundi = (int)(deadline - DateTime.UtcNow).TotalSeconds;
                }
            }

            // Sve završene sesije ovog studenta (najnovije prvo)
            var sesije = await _context.KvizSesije
                .Include(s => s.Predmet)
                .Where(s => s.StudentId == userId && s.Status == StatusSesije.ZAVRSEN)
                .OrderByDescending(s => s.DatumZavrsetka)
                .ToListAsync();

            model.KvizoviZavrseni = sesije.Count;
            model.ProsjecanRezultat = sesije.Count > 0
                ? (int)Math.Round(sesije.Average(s => s.Procenat))
                : 0;

            // XP iz stvarnih kvizova (10 XP po tačnom odgovoru)
            var brojTacnih = sesije.Sum(s => s.BrojTacnih);
            model.XpUkupno = brojTacnih * 10;

            // Progresivni nivoi: svaki nivo košta 100 XP više od prethodnog
            // (nivo 1→2 traži 100, 2→3 traži 200, 3→4 traži 300 ...)
            var nivo = 1;
            var preostaloXp = model.XpUkupno;
            var cijenaNivoa = 100;
            while (preostaloXp >= cijenaNivoa)
            {
                preostaloXp -= cijenaNivoa;
                nivo++;
                cijenaNivoa += 100;
            }
            model.Nivo = nivo;
            model.XpUNivou = preostaloXp;       // bodovi unutar trenutnog nivoa
            model.XpZaNivo = cijenaNivoa;        // koliko treba za sljedeći nivo

            // Rang među studentima koji su radili kvizove (po ukupno tačnih odgovora)
            var tacniPoStudentu = await _context.KvizSesije
                .Where(s => s.Status == StatusSesije.ZAVRSEN && s.StudentId != null)
                .GroupBy(s => s.StudentId!)
                .Select(g => new { StudentId = g.Key, Tacni = g.Sum(x => x.BrojTacnih) })
                .ToListAsync();

            model.UkupnoStudenata = tacniPoStudentu.Count;
            model.Rang = tacniPoStudentu.Count(x => x.Tacni > brojTacnih) + 1;

            // Nedavna aktivnost (zadnjih 5 kvizova)
            model.NedavneAktivnosti = sesije
                .Take(5)
                .Select(s => new NedavnaAktivnostViewModel
                {
                    PredmetNaziv = s.Predmet != null ? s.Predmet.Naziv : "Nepoznat predmet",
                    Procenat = s.Procenat,
                    Datum = s.DatumZavrsetka
                })
                .ToList();

            // Broj pitanja po predmetu (za prikaz "X pitanja")
            var brojPitanjaPoPredmetu = await _context.Pitanje
                .GroupBy(p => p.PredmetId)
                .Select(g => new { PredmetId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.PredmetId, x => x.Broj);

            // Moji predmeti: prosječan procenat po predmetu koji je student radio
            model.MojiPredmeti = sesije
                .Where(s => s.Predmet != null)
                .GroupBy(s => s.Predmet!)
                .Select(g => new PredmetNapredakViewModel
                {
                    Naziv = g.Key.Naziv,
                    Procenat = (int)Math.Round(g.Average(s => s.Procenat)),
                    BrojPitanja = brojPitanjaPoPredmetu.TryGetValue(g.Key.Id, out var b) ? b : 0
                })
                .ToList();

            return View(model);
        }

        // GET: /Student/Prepare
        public async Task<IActionResult> Prepare()
        {
            ViewBag.Predmeti = await _context.Predmet
                .OrderBy(p => p.Naziv)
                .Select(p => new { p.Id, p.Naziv })
                .ToListAsync();

            return View();
        }

        // GET: /Student/PrepareQuestions?predmetId=5  — JSON pitanja za vježbu (bez tajmera)
        public async Task<IActionResult> PrepareQuestions(int predmetId)
        {
            var pitanja = await _context.Pitanje
                .Where(p => p.PredmetId == predmetId)
                .ToListAsync();

            var ids = pitanja.Select(p => p.Id).ToList();
            var sviOdgovori = await _context.Odgovor
                .Where(o => ids.Contains(o.PitanjeId))
                .ToListAsync();

            var poPitanju = sviOdgovori
                .GroupBy(o => o.PitanjeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var rezultat = new List<object>();

            foreach (var p in pitanja)
            {
                if (!poPitanju.TryGetValue(p.Id, out var ods)) continue;

                // validno pitanje: bar 2 odgovora i tačno jedan tačan
                if (ods.Count < 2 || ods.Count(o => o.IsTacan) != 1) continue;

                var promijesani = Shuffle(ods);
                var tacanIndex = promijesani.FindIndex(o => o.IsTacan);

                rezultat.Add(new
                {
                    tekst = p.TekstPitanja,
                    opcije = promijesani.Select(o => o.Tekst).ToList(),
                    tacan = tacanIndex
                });
            }

            return Json(Shuffle(rezultat));
        }

        // GET: /Student/Simulate
        public async Task<IActionResult> Simulate()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var model = new SimulacijaKvizaViewModel
            {
                Predmeti = await GetPredmetiZaSimulacijuAsync()
            };

            var aktivnaSesija = await _context.KvizSesije
                .Include(s => s.Predmet)
                .Where(s => s.StudentId == userId && s.Status == StatusSesije.U_TOKU)
                .OrderByDescending(s => s.DatumPocetka)
                .FirstOrDefaultAsync();

            if (aktivnaSesija != null)
            {
                var deadline = aktivnaSesija.DatumPocetka.AddMinutes(aktivnaSesija.VremenskoOgranicenje);

                if (DateTime.UtcNow >= deadline)
                {
                    var rezultat = await FinishSimulationAndBuildResultAsync(aktivnaSesija.ID);

                    model.Result = rezultat;
                    model.PredmetId = aktivnaSesija.PredmetId;
                    model.PredmetNaziv = aktivnaSesija.Predmet?.Naziv ?? "Odabrani predmet";
                    model.BrojPitanja = rezultat?.UkupnoPitanja ?? 0;

                    return View(model);
                }

                model.AktivnaSesijaId = aktivnaSesija.ID;
                model.AktivnaSesijaPredmetNaziv = aktivnaSesija.Predmet?.Naziv ?? "Odabrani predmet";
                model.PreostaloSekundi = (int)(deadline - DateTime.UtcNow).TotalSeconds;
            }

            if (TempData["SimulationError"] is string error)
            {
                model.ErrorMessage = error;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartSimulation(SimulacijaKvizaViewModel model)
        {
            if (!model.PredmetId.HasValue)
            {
                TempData["SimulationError"] = "Moraš odabrati predmet prije pokretanja simulacije.";
                return RedirectToAction(nameof(Simulate));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var aktivneSesije = await _context.KvizSesije
                .Where(s =>
                    s.StudentId == userId &&
                    s.Status == StatusSesije.U_TOKU)
                .OrderByDescending(s => s.DatumPocetka)
                .ToListAsync();

            foreach (var sesija in aktivneSesije)
            {
                var deadline = sesija.DatumPocetka.AddMinutes(sesija.VremenskoOgranicenje);

                if (now < deadline)
                {
                    TempData["SimulationError"] = "Već imaš aktivnu simulaciju kviza. Prvo nastavi postojeći kviz ili sačekaj da vrijeme istekne.";
                    return RedirectToAction(nameof(Simulate));
                }

                await FinishSimulationAndBuildResultAsync(sesija.ID);
            }

            var simulationModel = await BuildSimulationAsync(model.PredmetId.Value);

            if (!string.IsNullOrWhiteSpace(simulationModel.ErrorMessage))
            {
                TempData["SimulationError"] = simulationModel.ErrorMessage;
                return RedirectToAction(nameof(Simulate));
            }

            return View("Simulate", simulationModel);
        }

        public async Task<IActionResult> ContinueSimulation(int kvizSesijaId)
        {
            var model = await BuildActiveSimulationModelAsync(kvizSesijaId);

            if (model.Result != null)
            {
                return View("Simulate", model);
            }

            if (!string.IsNullOrWhiteSpace(model.ErrorMessage))
            {
                TempData["SimulationError"] = model.ErrorMessage;
                return RedirectToAction(nameof(Simulate));
            }

            return View("Simulate", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSimulation(SimulacijaSubmitViewModel submitModel)
        {
            if (submitModel.KvizSesijaId <= 0)
            {
                TempData["SimulationError"] = "Simulacija nije validna.";
                return RedirectToAction(nameof(Simulate));
            }

            var rezultat = await FinishSimulationAndBuildResultAsync(submitModel.KvizSesijaId);

            if (rezultat == null)
            {
                TempData["SimulationError"] = "Sesija simulacije nije pronađena.";
                return RedirectToAction(nameof(Simulate));
            }

            var sesija = await _context.KvizSesije
                .Include(s => s.Predmet)
                .FirstAsync(s => s.ID == submitModel.KvizSesijaId);

            var model = new SimulacijaKvizaViewModel
            {
                KvizSesijaId = sesija.ID,
                PredmetId = sesija.PredmetId,
                PredmetNaziv = sesija.Predmet?.Naziv ?? "Odabrani predmet",
                BrojPitanja = rezultat.UkupnoPitanja,
                VremenskoOgranicenjeMinuta = sesija.VremenskoOgranicenje,
                Result = rezultat,
                Predmeti = await GetPredmetiZaSimulacijuAsync(sesija.PredmetId)
            };

            return View("Simulate", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSimulationAnswer(SaveSimulationAnswerViewModel answerModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sesija = await _context.KvizSesije
                .FirstOrDefaultAsync(s =>
                    s.ID == answerModel.KvizSesijaId &&
                    s.StudentId == userId &&
                    s.Status == StatusSesije.U_TOKU);

            if (sesija == null)
            {
                return BadRequest("Aktivna sesija nije pronađena.");
            }

            var deadline = sesija.DatumPocetka.AddMinutes(sesija.VremenskoOgranicenje);

            if (DateTime.UtcNow >= deadline)
            {
                await FinishSimulationAndBuildResultAsync(sesija.ID);
                return BadRequest("Vrijeme je isteklo.");
            }

            var sesijaPitanje = await _context.KvizSesijaPitanja
                .FirstOrDefaultAsync(x =>
                    x.KvizSesijaId == sesija.ID &&
                    x.PitanjeId == answerModel.PitanjeId);

            if (sesijaPitanje == null)
            {
                return BadRequest("Pitanje nije dio ove sesije.");
            }

            if (sesijaPitanje.OdgovorId.HasValue)
            {
                return BadRequest("Odgovor je već potvrđen.");
            }

            var odgovor = await _context.Odgovor
                .FirstOrDefaultAsync(o =>
                    o.Id == answerModel.OdgovorId &&
                    o.PitanjeId == answerModel.PitanjeId);

            if (odgovor == null)
            {
                return BadRequest("Odgovor nije validan.");
            }

            sesijaPitanje.OdgovorId = odgovor.Id;
            sesijaPitanje.Tacno = odgovor.IsTacan ? 1 : 0;
            sesijaPitanje.BrojBodova = odgovor.IsTacan ? 1 : 0;

            await _context.SaveChangesAsync();

            return Json(new { saved = true });
        }

        // GET: Student
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Student/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BrojOdgovorenihPitanja,BrojTacnihOdgovora,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Student/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("BrojOdgovorenihPitanja,BrojTacnihOdgovora,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var student = await _context.Student.FindAsync(id);
            if (student != null)
            {
                _context.Student.Remove(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
