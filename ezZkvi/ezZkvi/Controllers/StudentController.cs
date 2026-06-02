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
        private const int VremenskoOgranicenjeMinuta = 15;

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
        public IActionResult Prepare()
        {
            return View();
        }

        // GET: /Student/Simulate
        public async Task<IActionResult> Simulate()
        {
            var model = new SimulacijaKvizaViewModel
            {
                Predmeti = await GetPredmetiZaSimulacijuAsync()
            };

            if (TempData["SimulationError"] is string error)
            {
                model.ErrorMessage = error;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartSimulation(int? predmetId)
        {
            if (!predmetId.HasValue || predmetId.Value <= 0)
            {
                var emptyModel = new SimulacijaKvizaViewModel
                {
                    ErrorMessage = "Moraš odabrati predmet prije pokretanja simulacije.",
                    Predmeti = await GetPredmetiZaSimulacijuAsync()
                };

                return View("Simulate", emptyModel);
            }

            var model = await BuildSimulationAsync(predmetId.Value);

            if (!string.IsNullOrWhiteSpace(model.ErrorMessage))
            {
                model.Predmeti = await GetPredmetiZaSimulacijuAsync(predmetId.Value);
                return View("Simulate", model);
            }

            return View("Simulate", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSimulation(SimulacijaSubmitViewModel submitModel)
        {
            if (submitModel.Odgovori.Count == 0)
            {
                TempData["SimulationError"] = "Simulacija nije validna jer nema odabranih pitanja.";
                return RedirectToAction(nameof(Simulate));
            }

            var selectedQuestionIds = submitModel.Odgovori
                .Select(o => o.PitanjeId)
                .Distinct()
                .ToList();

            var pitanja = await _context.Pitanje
                .Where(p => selectedQuestionIds.Contains(p.Id) && p.PredmetId == submitModel.PredmetId)
                .ToListAsync();

            var odgovori = await _context.Odgovor
                .Where(o => selectedQuestionIds.Contains(o.PitanjeId))
                .ToListAsync();

            if (pitanja.Count == 0)
            {
                TempData["SimulationError"] = "Pitanja za ovu simulaciju nisu pronađena.";
                return RedirectToAction(nameof(Simulate));
            }

            var rezultat = CalculateResult(submitModel, pitanja, odgovori);

            await SaveSimulationResultAsync(rezultat, submitModel.PredmetId);

            var predmet = await _context.Predmet.FindAsync(submitModel.PredmetId);

            var model = new SimulacijaKvizaViewModel
            {
                PredmetId = submitModel.PredmetId,
                PredmetNaziv = predmet?.Naziv ?? "Odabrani predmet",
                BrojPitanja = rezultat.UkupnoPitanja,
                VremenskoOgranicenjeMinuta = submitModel.TotalSeconds > 0 ? submitModel.TotalSeconds / 60 : 15,
                Result = rezultat,
                Predmeti = await GetPredmetiZaSimulacijuAsync(submitModel.PredmetId)
            };

            return View("Simulate", model);
        }


        // GET: /Student/Leaderboard
        public async Task<IActionResult> Leaderboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Agregiraj rezultate po studentu DIREKTNO iz završenih kvizova
            var statistika = await _context.KvizSesije
                .Where(s => s.Status == StatusSesije.ZAVRSEN && s.StudentId != null)
                .GroupBy(s => s.StudentId!)
                .Select(g => new
                {
                    StudentId = g.Key,
                    Kvizovi = g.Count(),
                    UkupnoTacnih = g.Sum(x => x.BrojTacnih),
                    UkupnoPitanja = g.Sum(x => x.TraziBrojPitanja)
                })
                .ToListAsync();

            // Povuci imena (UserName) za te studente iz tabele svih korisnika
            var ids = statistika.Select(s => s.StudentId).ToList();
            var korisnici = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var entries = statistika
                .Select(s =>
                {
                    korisnici.TryGetValue(s.StudentId, out var userName);

                    var ime = !string.IsNullOrEmpty(userName) && userName.Contains('@')
                        ? userName.Split('@')[0]
                        : (userName ?? "Student");

                    var inicijali = ime.Length >= 2
                        ? ime.Substring(0, 2).ToUpper()
                        : ime.ToUpper();

                    var tacnost = s.UkupnoPitanja > 0
                        ? (int)Math.Round((double)s.UkupnoTacnih / s.UkupnoPitanja * 100)
                        : 0;

                    return new LeaderboardEntryViewModel
                    {
                        Ime = ime,
                        Inicijali = inicijali,
                        Bodovi = s.UkupnoTacnih * 10,
                        Tacnost = tacnost,
                        Kvizovi = s.Kvizovi,
                        JeTrenutni = s.StudentId == userId
                    };
                })
                .OrderByDescending(e => e.Bodovi)
                .ThenByDescending(e => e.Tacnost)
                .ToList();

            return View(new LeaderboardViewModel { Entries = entries });
        }

        // GET: Student
        public async Task<IActionResult> Index()
        {
            return View(await _context.Student.ToListAsync());
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
