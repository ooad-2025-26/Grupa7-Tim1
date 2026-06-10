using ezZkvi.Data;
using ezZkvi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class OdgovorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OdgovorController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<bool> SmijePitanjeAsync(int pitanjeId)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.Pitanje
                .Include(p => p.Predmet)
                .AnyAsync(p => p.Id == pitanjeId && p.Predmet != null && p.Predmet.KreatorId == userId);
        }

        private async Task<bool> SmijeOdgovorAsync(int odgovorId)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.Odgovor
                .Include(o => o.Pitanje)
                .ThenInclude(p => p.Predmet)
                .AnyAsync(o =>
                    o.Id == odgovorId &&
                    o.Pitanje != null &&
                    o.Pitanje.Predmet != null &&
                    o.Pitanje.Predmet.KreatorId == userId);
        }

        private IQueryable<Pitanje> DozvoljenaPitanjaQuery()
        {
            var query = _context.Pitanje.AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(p => p.Predmet != null && p.Predmet.KreatorId == userId);
            }

            return query.OrderBy(p => p.TekstPitanja);
        }

        public async Task<IActionResult> Index()
        {
            var query = _context.Odgovor
                .Include(o => o.Pitanje)
                .ThenInclude(p => p.Predmet)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(o =>
                    o.Pitanje != null &&
                    o.Pitanje.Predmet != null &&
                    o.Pitanje.Predmet.KreatorId == userId);
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odgovor = await _context.Odgovor
                .Include(o => o.Pitanje)
                .ThenInclude(p => p.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (odgovor == null)
            {
                return NotFound();
            }

            if (!await SmijeOdgovorAsync(odgovor.Id))
            {
                return Forbid();
            }

            return View(odgovor);
        }

        public IActionResult Create()
        {
            ViewData["PitanjeId"] = new SelectList(DozvoljenaPitanjaQuery(), "Id", "TekstPitanja");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tekst,IsTacan,PitanjeId")] Odgovor odgovor)
        {
            if (!await SmijePitanjeAsync(odgovor.PitanjeId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewData["PitanjeId"] = new SelectList(DozvoljenaPitanjaQuery(), "Id", "TekstPitanja", odgovor.PitanjeId);
                return View(odgovor);
            }

            _context.Add(odgovor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odgovor = await _context.Odgovor.FindAsync(id.Value);

            if (odgovor == null)
            {
                return NotFound();
            }

            if (!await SmijeOdgovorAsync(odgovor.Id))
            {
                return Forbid();
            }

            ViewData["PitanjeId"] = new SelectList(DozvoljenaPitanjaQuery(), "Id", "TekstPitanja", odgovor.PitanjeId);
            return View(odgovor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tekst,IsTacan,PitanjeId")] Odgovor odgovor)
        {
            if (id != odgovor.Id)
            {
                return NotFound();
            }

            var postojeci = await _context.Odgovor.FindAsync(id);

            if (postojeci == null)
            {
                return NotFound();
            }

            if (!await SmijeOdgovorAsync(postojeci.Id))
            {
                return Forbid();
            }

            if (!await SmijePitanjeAsync(odgovor.PitanjeId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewData["PitanjeId"] = new SelectList(DozvoljenaPitanjaQuery(), "Id", "TekstPitanja", odgovor.PitanjeId);
                return View(odgovor);
            }

            postojeci.Tekst = odgovor.Tekst;
            postojeci.IsTacan = odgovor.IsTacan;
            postojeci.PitanjeId = odgovor.PitanjeId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odgovor = await _context.Odgovor
                .Include(o => o.Pitanje)
                .ThenInclude(p => p.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (odgovor == null)
            {
                return NotFound();
            }

            if (!await SmijeOdgovorAsync(odgovor.Id))
            {
                return Forbid();
            }

            return View(odgovor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var odgovor = await _context.Odgovor.FindAsync(id);

            if (odgovor == null)
            {
                return NotFound();
            }

            if (!await SmijeOdgovorAsync(odgovor.Id))
            {
                return Forbid();
            }

            _context.Odgovor.Remove(odgovor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
