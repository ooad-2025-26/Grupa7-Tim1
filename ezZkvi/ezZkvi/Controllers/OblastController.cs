using System.Security.Claims;
using ezZkvi.Data;
using ezZkvi.Models;
using ezZkvi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Admin,Moderator,Student")]
    public class OblastController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OblastController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool SmijePredmet(Predmet predmet)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Student")) return true;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return predmet.KreatorId == userId;
        }

        private async Task<bool> SmijeOblastAsync(int oblastId)
        {
            var oblast = await _context.Oblast
                .Include(o => o.Predmet)
                .FirstOrDefaultAsync(o => o.Id == oblastId);

            return oblast != null && oblast.Predmet != null && SmijePredmet(oblast.Predmet);
        }

        private IActionResult RedirectNaContentOblasti()
        {
            return RedirectToAction("Index", "Content", new { tab = "areas" });
        }

        private async Task<bool> PostojiOblastSaNazivomAsync(int predmetId, string naziv, int? ignorisiId = null)
        {
            var kljuc = ContentValidation.KljucZaPoredjenje(naziv);

            return await _context.Oblast
                .AnyAsync(o =>
                    o.PredmetId == predmetId &&
                    (!ignorisiId.HasValue || o.Id != ignorisiId.Value) &&
                    o.Naziv.Trim().ToLower() == kljuc);
        }

        public async Task<IActionResult> ByPredmet(int predmetId)
        {
            var predmet = await _context.Predmet.FindAsync(predmetId);
            if (predmet == null || !SmijePredmet(predmet))
            {
                return Json(Array.Empty<object>());
            }

            var oblasti = await _context.Oblast
                .Where(o => o.PredmetId == predmetId)
                .OrderBy(o => o.Naziv)
                .Select(o => new { o.Id, o.Naziv, o.PredmetId })
                .ToListAsync();

            return Json(oblasti);
        }

        public IActionResult Index()
        {
            return RedirectNaContentOblasti();
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromContent(int predmetId, string naziv)
        {
            var predmet = await _context.Predmet.FindAsync(predmetId);
            if (predmet == null)
            {
                TempData["Error"] = "Predmet za oblast nije pronađen.";
                return RedirectNaContentOblasti();
            }

            if (!SmijePredmet(predmet))
            {
                return Forbid();
            }

            var nazivOblasti = ContentValidation.NormalizujUnos(naziv);

            if (nazivOblasti.Length < 2 || nazivOblasti.Length > 100)
            {
                TempData["Error"] = "Naziv oblasti mora imati između 2 i 100 karaktera.";
                return RedirectNaContentOblasti();
            }

            if (!ContentValidation.NazivImaDozvoljeneZnakove(nazivOblasti))
            {
                TempData["Error"] = "Naziv oblasti smije sadržavati samo slova, brojeve i razmake.";
                return RedirectNaContentOblasti();
            }

            if (await PostojiOblastSaNazivomAsync(predmetId, nazivOblasti))
            {
                TempData["Error"] = "Oblast sa istim nazivom već postoji u ovom predmetu.";
                return RedirectNaContentOblasti();
            }

            _context.Oblast.Add(new Oblast
            {
                Naziv = nazivOblasti,
                PredmetId = predmetId
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oblast je uspješno dodana.";
            return RedirectNaContentOblasti();
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFromContent(int id, int predmetId, string naziv)
        {
            var oblast = await _context.Oblast
                .Include(o => o.Predmet)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (oblast == null)
            {
                return NotFound();
            }

            if (!await SmijeOblastAsync(oblast.Id))
            {
                return Forbid();
            }

            var noviPredmet = await _context.Predmet.FindAsync(predmetId);
            if (noviPredmet == null || !SmijePredmet(noviPredmet))
            {
                return Forbid();
            }

            var nazivOblasti = ContentValidation.NormalizujUnos(naziv);

            if (nazivOblasti.Length < 2 || nazivOblasti.Length > 100)
            {
                TempData["Error"] = "Naziv oblasti mora imati između 2 i 100 karaktera.";
                return RedirectNaContentOblasti();
            }

            if (!ContentValidation.NazivImaDozvoljeneZnakove(nazivOblasti))
            {
                TempData["Error"] = "Naziv oblasti smije sadržavati samo slova, brojeve i razmake.";
                return RedirectNaContentOblasti();
            }

            if (await PostojiOblastSaNazivomAsync(predmetId, nazivOblasti, oblast.Id))
            {
                TempData["Error"] = "Oblast sa istim nazivom već postoji u ovom predmetu.";
                return RedirectNaContentOblasti();
            }

            if (oblast.PredmetId != predmetId)
            {
                var imaPitanja = await _context.Pitanje.AnyAsync(p => p.OblastId == id);
                var imaSesija = await _context.KvizSesije.AnyAsync(s => s.OblastId == id);

                if (imaPitanja || imaSesija)
                {
                    TempData["Error"] = "Oblast se ne može prebaciti na drugi predmet jer ima pitanja ili historiju kvizova.";
                    return RedirectNaContentOblasti();
                }
            }

            oblast.Naziv = nazivOblasti;
            oblast.PredmetId = predmetId;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oblast je ažurirana.";
            return RedirectNaContentOblasti();
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromContent(int id)
        {
            var oblast = await _context.Oblast
                .Include(o => o.Predmet)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (oblast == null)
            {
                return NotFound();
            }

            if (!await SmijeOblastAsync(oblast.Id))
            {
                return Forbid();
            }

            await ContentDeletionService.ObrisiOblastSaSadrzajemAsync(_context, id);
            _context.Oblast.Remove(oblast);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oblast je obrisana zajedno sa povezanim pitanjima i kviz sesijama.";
            return RedirectNaContentOblasti();
        }
    }
}
