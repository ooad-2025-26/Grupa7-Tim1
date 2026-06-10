using System.Security.Claims;
using ezZkvi.Data;
using ezZkvi.Models;
using ezZkvi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class ModeratorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModeratorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Vraća null ako korisnik smije SVE (Admin), inače listu ID-eva predmeta koje je on kreirao
        private async Task<List<int>?> DozvoljeniPredmetiIdAsync()
        {
            if (User.IsInRole("Admin"))
            {
                return null;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Predmet
                .Where(p => p.KreatorId == userId)
                .Select(p => p.Id)
                .ToListAsync();
        }

        private async Task<bool> SmijeFeedbackAsync(Feedback feedback)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            if (feedback.PredmetId == null)
            {
                return true;
            }

            var dozvoljeni = await DozvoljeniPredmetiIdAsync();
            return dozvoljeni != null && dozvoljeni.Contains(feedback.PredmetId.Value);
        }

        private bool ModeratorExists(string id)
        {
            return _context.Moderator.Any(e => e.Id == id);
        }

        // GET: /Moderator/Dashboard  — samo Moderator (Admin ima svoj Administrator/Dashboard)
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> Dashboard()
        {
            var dozvoljeni = await DozvoljeniPredmetiIdAsync();

            // Predmeti / pitanja / feedback skalirani: admin sve, moderator samo svoje
            var predmetiQuery = _context.Predmet.AsQueryable();
            var pitanjaQuery = _context.Pitanje.AsQueryable();
            var feedbackQuery = _context.Feedback.AsQueryable();
            if (dozvoljeni != null)
            {
                predmetiQuery = predmetiQuery.Where(p => dozvoljeni.Contains(p.Id));
                pitanjaQuery = pitanjaQuery.Where(p => dozvoljeni.Contains(p.PredmetId));
                feedbackQuery = feedbackQuery.Where(f => f.PredmetId == null || dozvoljeni.Contains(f.PredmetId.Value));
            }

            // Statistike (kartice)
            ViewBag.BrojPitanja = await pitanjaQuery.CountAsync();
            ViewBag.BrojPredmeta = await predmetiQuery.CountAsync();
            ViewBag.NeobradeniFeedback = await feedbackQuery
                .CountAsync(f => f.Status == StatusFeedbacka.NA_CEKANJU);
            ViewBag.ObradeniFeedback = await feedbackQuery
                .CountAsync(f => f.Status != StatusFeedbacka.NA_CEKANJU);

            // Najnoviji feedback za obradu
            ViewBag.NajnovijiFeedback = (await feedbackQuery
                .Where(f => f.Status == StatusFeedbacka.NA_CEKANJU)
                .OrderByDescending(f => f.DatumSlanja)
                .Take(5)
                .ToListAsync())
                .Select(f => new FeedbackItem
                {
                    Sadrzaj = f.Sadrzaj,
                    Tip = f.TipFeedbacka,
                    Datum = f.DatumSlanja
                })
                .ToList();

            // Pitanja po predmetu
            var pitanjaPoPredmetu = await pitanjaQuery
                .GroupBy(p => p.PredmetId)
                .Select(g => new { PredmetId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.PredmetId, x => x.Broj);

            var predmeti = await predmetiQuery.ToListAsync();

            ViewBag.PitanjaPoPredmetu = predmeti
                .Select(p => new PredmetAktivnostItem
                {
                    Naziv = p.Naziv,
                    BrojPitanja = pitanjaPoPredmetu.TryGetValue(p.Id, out var b) ? b : 0
                })
                .OrderByDescending(p => p.BrojPitanja)
                .ToList();

            // Nedavno obrađeni feedback (zamjena za log)
            ViewBag.ObradjeniFeedback = (await feedbackQuery
                .Where(f => f.Status != StatusFeedbacka.NA_CEKANJU)
                .OrderByDescending(f => f.DatumSlanja)
                .Take(5)
                .ToListAsync())
                .Select(f => new FeedbackItem
                {
                    Sadrzaj = f.Sadrzaj,
                    Tip = f.TipFeedbacka,
                    Datum = f.DatumSlanja
                })
                .ToList();

            return View();
        }

        // GET: /Moderator/Content
        public async Task<IActionResult> Content(string? search, int? predmetId, Tezina? tezina)
        {
            var dozvoljeni = await DozvoljeniPredmetiIdAsync();

            var pitanjaQuery = _context.Pitanje
                .Include(p => p.Predmet)
                .AsQueryable();

            if (dozvoljeni != null)
            {
                pitanjaQuery = pitanjaQuery.Where(p => dozvoljeni.Contains(p.PredmetId));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                pitanjaQuery = pitanjaQuery.Where(p => p.TekstPitanja.Contains(search));
            }

            if (predmetId.HasValue)
            {
                pitanjaQuery = pitanjaQuery.Where(p => p.PredmetId == predmetId);
            }

            if (tezina.HasValue)
            {
                pitanjaQuery = pitanjaQuery.Where(p => p.Tezina == tezina.Value);
            }

            var predmetiQuery = _context.Predmet.AsQueryable();
            if (dozvoljeni != null)
            {
                predmetiQuery = predmetiQuery.Where(p => dozvoljeni.Contains(p.Id));
            }

            ViewData["Predmeti"] = new SelectList(predmetiQuery, "Id", "Naziv", predmetId);
            ViewData["Search"] = search;
            ViewData["Tezina"] = tezina;

            // Lista predmeta sa brojem pitanja (za tab "Predmeti")
            var brojPitanjaPoPredmetu = await _context.Pitanje
                .GroupBy(p => p.PredmetId)
                .Select(g => new { PredmetId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.PredmetId, x => x.Broj);

            ViewBag.PredmetiLista = (await predmetiQuery.OrderBy(p => p.Naziv).ToListAsync())
                .Select(p => new PredmetAktivnostItem
                {
                    Id = p.Id,
                    Naziv = p.Naziv,
                    BrojPitanja = brojPitanjaPoPredmetu.TryGetValue(p.Id, out var b) ? b : 0
                })
                .ToList();

            var pitanja = await pitanjaQuery
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(pitanja);
        }

        // GET: /Moderator/Feedback
        public async Task<IActionResult> Feedback()
        {
            var dozvoljeni = await DozvoljeniPredmetiIdAsync();

            var feedbackQuery = _context.Feedback
                .Include(f => f.Predmet)
                .AsQueryable();

            // Moderator vidi feedback za SVOJE predmete + opće prijave (bez predmeta); Admin vidi sve
            if (dozvoljeni != null)
            {
                feedbackQuery = feedbackQuery.Where(f => f.PredmetId == null || dozvoljeni.Contains(f.PredmetId.Value));
            }

            var sviFeedback = await feedbackQuery
                .OrderByDescending(f => f.DatumSlanja)
                .ToListAsync();

            // Imena autora (UserName) po KorisnikId
            var autorIds = sviFeedback
                .Where(f => f.KorisnikId != null)
                .Select(f => f.KorisnikId!)
                .Distinct()
                .ToList();

            ViewBag.Autori = await _context.Users
                .Where(u => autorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            ViewBag.Neobradjenih = sviFeedback.Count(f => f.Status == StatusFeedbacka.NA_CEKANJU);
            ViewBag.Obradjenih = sviFeedback.Count(f => f.Status != StatusFeedbacka.NA_CEKANJU);
            ViewBag.Prijedloga = sviFeedback.Count(f => f.TipFeedbacka == TipFeedbacka.PRIJEDLOG_PITANJA);
            ViewBag.PrijavaGresaka = sviFeedback.Count(f => f.TipFeedbacka == TipFeedbacka.PRIJAVA_GRESKE);

            return View(sviFeedback);
        }

        // POST: /Moderator/PrihvatiFeedback/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrihvatiFeedback(int id)
        {
            var fb = await _context.Feedback.FindAsync(id);

            if (fb == null)
            {
                return NotFound();
            }

            if (!await SmijeFeedbackAsync(fb))
            {
                return Forbid();
            }

            fb.Status = StatusFeedbacka.ODOBREN;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Feedback je prihvaćen.";
            return RedirectToAction(nameof(Feedback));
        }

        // POST: /Moderator/OdbijFeedback/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdbijFeedback(int id)
        {
            var fb = await _context.Feedback.FindAsync(id);

            if (fb == null)
            {
                return NotFound();
            }

            if (!await SmijeFeedbackAsync(fb))
            {
                return Forbid();
            }

            fb.Status = StatusFeedbacka.ODBIJEN;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Feedback je odbijen.";
            return RedirectToAction(nameof(Feedback));
        }

        // GET: Moderator
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Content));
            }

            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult Details(string id)
        {
            return Forbid();
        }

        public IActionResult Create()
        {
            return Forbid();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Moderator moderator)
        {
            return Forbid();
        }

        public IActionResult Edit(string id)
        {
            return Forbid();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Moderator moderator)
        {
            return Forbid();
        }

        public IActionResult Delete(string id)
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
