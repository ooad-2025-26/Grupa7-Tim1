using System.Security.Claims;
using ezZkvi.Data;
using ezZkvi.Models;
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
            return RedirectToAction("Index", "Content");
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
                return RedirectToAction("Index", "Content");
            }

            if (!SmijePredmet(predmet))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(naziv) || naziv.Trim().Length < 2)
            {
                TempData["Error"] = "Naziv oblasti je obavezan.";
                return RedirectToAction("Index", "Content");
            }

            _context.Oblast.Add(new Oblast
            {
                Naziv = naziv.Trim(),
                PredmetId = predmetId
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oblast je uspješno dodana.";
            return RedirectToAction("Index", "Content");
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

            if (string.IsNullOrWhiteSpace(naziv) || naziv.Trim().Length < 2)
            {
                TempData["Error"] = "Naziv oblasti je obavezan.";
                return RedirectToAction("Index", "Content");
            }

            oblast.Naziv = naziv.Trim();
            oblast.PredmetId = predmetId;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oblast je ažurirana.";
            return RedirectToAction("Index", "Content");
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

            var imaPitanja = await _context.Pitanje.AnyAsync(p => p.OblastId == id);
            if (imaPitanja)
            {
                TempData["Error"] = "Oblast se ne može obrisati dok ima pitanja.";
                return RedirectToAction("Index", "Content");
            }

            _context.Oblast.Remove(oblast);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Oblast je obrisana.";
            return RedirectToAction("Index", "Content");
        }
    }
}
