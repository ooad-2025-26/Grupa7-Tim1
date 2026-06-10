using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ezZkvi.Data;
using ezZkvi.Models;

namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class PredmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PredmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin smije svaki predmet; moderator samo onaj koji je sam kreirao
        private bool SmijePredmet(Predmet predmet)
        {
            if (User.IsInRole("Admin")) return true;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return predmet.KreatorId == userId;
        }

        private async Task<bool> SmijeOblastAsync(int oblastId, int predmetId)
        {
            var oblast = await _context.Oblast
                .Include(o => o.Predmet)
                .FirstOrDefaultAsync(o => o.Id == oblastId && o.PredmetId == predmetId);

            return oblast != null && oblast.Predmet != null && SmijePredmet(oblast.Predmet);
        }

        // GET: /Predmet/ExportCsv/5  — skine sva pitanja predmeta kao CSV
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> ExportCsv(int id)
        {
            var predmet = await _context.Predmet.FindAsync(id);
            if (predmet == null)
            {
                TempData["Error"] = "Odaberi predmet za preuzimanje CSV-a.";
                return RedirectToAction("Index", "Content");
            }

            if (!SmijePredmet(predmet))
            {
                TempData["Error"] = "Nemaš pristup ovom predmetu.";
                return RedirectToAction("Index", "Content");
            }

            var pitanja = await _context.Pitanje
                .Include(p => p.Oblast)
                .Where(p => p.PredmetId == id)
                .OrderBy(p => p.Oblast != null ? p.Oblast.Naziv : "")
                .ThenBy(p => p.Id)
                .ToListAsync();

            var pitanjeIds = pitanja.Select(p => p.Id).ToList();
            var sviOdgovori = await _context.Odgovor
                .Where(o => pitanjeIds.Contains(o.PitanjeId))
                .OrderBy(o => o.Id)
                .ToListAsync();

            var odgovoriPoPitanju = sviOdgovori
                .GroupBy(o => o.PitanjeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var sb = new StringBuilder();
            sb.AppendLine("TekstPitanja,OdgovorA,OdgovorB,OdgovorC,OdgovorD,Tacan,Tezina,Oblast");

            foreach (var p in pitanja)
            {
                odgovoriPoPitanju.TryGetValue(p.Id, out var ods);
                ods ??= new List<Odgovor>();

                var a = ods.ElementAtOrDefault(0);
                var b = ods.ElementAtOrDefault(1);
                var c = ods.ElementAtOrDefault(2);
                var d = ods.ElementAtOrDefault(3);

                var tacan = a?.IsTacan == true ? "A"
                          : b?.IsTacan == true ? "B"
                          : c?.IsTacan == true ? "C"
                          : d?.IsTacan == true ? "D" : "";

                sb.AppendLine(string.Join(",", new[]
                {
                    CsvPolje(p.TekstPitanja),
                    CsvPolje(a?.Tekst), CsvPolje(b?.Tekst), CsvPolje(c?.Tekst), CsvPolje(d?.Tekst),
                    tacan,
                    p.Tezina.ToString(),
                    CsvPolje(p.Oblast?.Naziv)
                }));
            }

            // BOM + UTF-8 da Excel pravilno prikaže š, č, ć, ž, đ
            var bom = Encoding.UTF8.GetPreamble();
            var sadrzaj = Encoding.UTF8.GetBytes(sb.ToString());
            var bytes = bom.Concat(sadrzaj).ToArray();

            return File(bytes, "text/csv", $"pitanja_{predmet.Naziv}.csv");
        }

        // POST: /Predmet/ImportCsv  — učita pitanja iz CSV-a u bazu
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsv(int predmetId, int oblastId, IFormFile file)
        {
            var predmet = await _context.Predmet.FindAsync(predmetId);
            if (predmet == null)
            {
                TempData["Error"] = "Odaberi predmet za uvoz.";
                return RedirectToAction("Index", "Content");
            }

            if (!SmijePredmet(predmet))
            {
                TempData["Error"] = "Nemaš pristup ovom predmetu.";
                return RedirectToAction("Index", "Content");
            }

            if (!await SmijeOblastAsync(oblastId, predmetId))
            {
                TempData["Error"] = "Odabrana oblast ne pripada predmetu ili nemaš pristup.";
                return RedirectToAction("Index", "Content");
            }

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Odaberi CSV fajl.";
                return RedirectToAction("Index", "Content");
            }

            var dodano = 0;
            var preskoceno = 0;
            var prviRed = true;

            using var reader = new StreamReader(file.OpenReadStream());
            string? linija;

            while ((linija = await reader.ReadLineAsync()) != null)
            {
                if (prviRed) { prviRed = false; continue; }   // preskoči zaglavlje
                if (string.IsNullOrWhiteSpace(linija)) continue;

                var polja = ParsirajCsvLiniju(linija);
                if (polja.Count < 7) { preskoceno++; continue; }

                var tekst = polja[0].Trim();
                var tacan = polja[5].Trim().ToUpper();
                var tezinaStr = polja[6].Trim().ToUpper();

                var kandidati = new[]
                {
                    ("A", polja[1].Trim()),
                    ("B", polja[2].Trim()),
                    ("C", polja[3].Trim()),
                    ("D", polja[4].Trim())
                };
                var validni = kandidati.Where(k => !string.IsNullOrWhiteSpace(k.Item2)).ToList();

                // mora imati tekst, bar 2 odgovora i tačan odgovor koji postoji
                if (string.IsNullOrWhiteSpace(tekst) || validni.Count < 2 || !validni.Any(k => k.Item1 == tacan))
                {
                    preskoceno++;
                    continue;
                }

                if (!Enum.TryParse<Tezina>(tezinaStr, out var tezina))
                {
                    tezina = Tezina.SREDNJE;
                }

                var pitanje = new Pitanje
                {
                    TekstPitanja = tekst,
                    PredmetId = predmetId,
                    OblastId = oblastId,
                    Tezina = tezina
                };
                _context.Pitanje.Add(pitanje);
                await _context.SaveChangesAsync();

                var odgovori = validni.Select(k => new Odgovor
                {
                    Tekst = k.Item2,
                    PitanjeId = pitanje.Id,
                    IsTacan = k.Item1 == tacan
                });
                _context.Odgovor.AddRange(odgovori);
                await _context.SaveChangesAsync();

                dodano++;
            }

            TempData["Success"] = $"Uvezeno {dodano} pitanja."
                + (preskoceno > 0 ? $" Preskočeno {preskoceno} neispravnih redova." : "");
            return RedirectToAction("Index", "Content");
        }

        // Pretvori vrijednost u sigurno CSV polje (navodnici ako ima zarez/navodnik)
        private static string CsvPolje(string? vrijednost)
        {
            vrijednost ??= "";
            if (vrijednost.Contains(',') || vrijednost.Contains('"') || vrijednost.Contains('\n') || vrijednost.Contains('\r'))
            {
                return "\"" + vrijednost.Replace("\"", "\"\"") + "\"";
            }
            return vrijednost;
        }

        // Razbij jednu CSV liniju na polja (poštuje navodnike)
        private static List<string> ParsirajCsvLiniju(string linija)
        {
            var rezultat = new List<string>();
            var sb = new StringBuilder();
            var uNavodnicima = false;

            for (var i = 0; i < linija.Length; i++)
            {
                var ch = linija[i];

                if (uNavodnicima)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < linija.Length && linija[i + 1] == '"') { sb.Append('"'); i++; }
                        else uNavodnicima = false;
                    }
                    else sb.Append(ch);
                }
                else
                {
                    if (ch == '"') uNavodnicima = true;
                    else if (ch == ',') { rezultat.Add(sb.ToString()); sb.Clear(); }
                    else sb.Append(ch);
                }
            }
            rezultat.Add(sb.ToString());
            return rezultat;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromContent([Bind("Naziv")] Predmet predmet)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Predmet nije sačuvan. Provjeri naziv.";
                return RedirectToAction("Index", "Content");
            }

            if (!User.IsInRole("Admin"))
            {
                predmet.KreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            _context.Predmet.Add(predmet);
            await _context.SaveChangesAsync();

            _context.Oblast.Add(new Oblast
            {
                Naziv = "Oblast 1",
                PredmetId = predmet.Id
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Predmet je uspješno dodan.";
            return RedirectToAction("Index", "Content");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFromContent(int id, [Bind("Id,Naziv")] Predmet predmet)
        {
            if (id != predmet.Id)
            {
                return NotFound();
            }

            var postojeci = await _context.Predmet.FindAsync(id);
            if (postojeci == null)
            {
                return NotFound();
            }

            if (!SmijePredmet(postojeci))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(predmet.Naziv))
            {
                TempData["Error"] = "Naziv predmeta je obavezan.";
                return RedirectToAction("Index", "Content");
            }

            postojeci.Naziv = predmet.Naziv.Trim();
            await _context.SaveChangesAsync();

            TempData["Success"] = "Predmet je ažuriran.";
            return RedirectToAction("Index", "Content");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromContent(int id)
        {
            var predmet = await _context.Predmet.FindAsync(id);
            if (predmet == null)
            {
                return NotFound();
            }

            if (!SmijePredmet(predmet))
            {
                return Forbid();
            }

            var imaPitanja = await _context.Pitanje.AnyAsync(p => p.PredmetId == id);
            if (imaPitanja)
            {
                TempData["Error"] = "Predmet se ne može obrisati dok ima pitanja.";
                return RedirectToAction("Index", "Content");
            }

            var imaSesija = await _context.KvizSesije.AnyAsync(s => s.PredmetId == id);
            var imaFeedback = await _context.Feedback.AnyAsync(f => f.PredmetId == id);

            if (imaSesija || imaFeedback)
            {
                TempData["Error"] = "Predmet se ne može obrisati jer ima historiju kvizova ili feedback.";
                return RedirectToAction("Index", "Content");
            }

            var oblasti = await _context.Oblast.Where(o => o.PredmetId == id).ToListAsync();
            _context.Oblast.RemoveRange(oblasti);
            _context.Predmet.Remove(predmet);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Predmet je obrisan.";
            return RedirectToAction("Index", "Content");
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Content");
        }

        public IActionResult Details(int? id)
        {
            return Forbid();
        }

        public IActionResult Create()
        {
            return Forbid();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Naziv")] Predmet predmet)
        {
            return Forbid();
        }

        public IActionResult Edit(int? id)
        {
            return Forbid();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Naziv")] Predmet predmet)
        {
            return Forbid();
        }

        public IActionResult Delete(int? id)
        {
            return Forbid();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return Forbid();
        }
    }
}
