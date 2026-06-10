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
    [Authorize(Roles = "Admin,Moderator")]
    public class PitanjeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PitanjeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<bool> SmijePredmetAsync(int predmetId)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.Predmet
                .AnyAsync(p => p.Id == predmetId && p.KreatorId == userId);
        }

        private async Task<bool> SmijeOblastAsync(int oblastId, int predmetId)
        {
            var oblast = await _context.Oblast
                .Include(o => o.Predmet)
                .FirstOrDefaultAsync(o => o.Id == oblastId && o.PredmetId == predmetId);

            if (oblast == null)
            {
                return false;
            }

            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return oblast.Predmet != null && oblast.Predmet.KreatorId == userId;
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

        private IQueryable<Predmet> DozvoljeniPredmetiQuery()
        {
            var query = _context.Predmet.AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(p => p.KreatorId == userId);
            }

            return query.OrderBy(p => p.Naziv);
        }

        private IQueryable<Oblast> DozvoljeneOblastiQuery()
        {
            var query = _context.Oblast.Include(o => o.Predmet).AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(o => o.Predmet != null && o.Predmet.KreatorId == userId);
            }

            return query.OrderBy(o => o.Predmet!.Naziv).ThenBy(o => o.Naziv);
        }

        private async Task NapuniSelectListeAsync(int? predmetId = null, int? oblastId = null)
        {
            ViewData["PredmetId"] = new SelectList(
                await DozvoljeniPredmetiQuery().ToListAsync(),
                "Id",
                "Naziv",
                predmetId
            );

            var oblasti = await DozvoljeneOblastiQuery()
                .Select(o => new
                {
                    o.Id,
                    Naziv = (o.Predmet != null ? o.Predmet.Naziv : "Predmet") + " / " + o.Naziv
                })
                .ToListAsync();

            ViewData["OblastId"] = new SelectList(oblasti, "Id", "Naziv", oblastId);
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
        public IActionResult Create(PitanjeSaOdgovorimaViewModel model)
        {
            return Forbid();
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pitanje = await _context.Pitanje
                .Include(p => p.Predmet)
                .Include(p => p.Oblast)
                .FirstOrDefaultAsync(p => p.Id == id.Value);

            if (pitanje == null)
            {
                return NotFound();
            }

            if (!await SmijePitanjeAsync(pitanje.Id))
            {
                return Forbid();
            }

            await NapuniSelectListeAsync(pitanje.PredmetId, pitanje.OblastId);
            return View(pitanje);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TekstPitanja,Tezina,PredmetId,OblastId")] Pitanje pitanje)
        {
            if (id != pitanje.Id)
            {
                return NotFound();
            }

            var postojeci = await _context.Pitanje.FindAsync(id);

            if (postojeci == null)
            {
                return NotFound();
            }

            if (!await SmijePitanjeAsync(postojeci.Id))
            {
                return Forbid();
            }

            if (!await SmijePredmetAsync(pitanje.PredmetId) || !await SmijeOblastAsync(pitanje.OblastId, pitanje.PredmetId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await NapuniSelectListeAsync(pitanje.PredmetId, pitanje.OblastId);
                return View(pitanje);
            }

            postojeci.TekstPitanja = pitanje.TekstPitanja;
            postojeci.Tezina = pitanje.Tezina;
            postojeci.PredmetId = pitanje.PredmetId;
            postojeci.OblastId = pitanje.OblastId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Pitanje je ažurirano.";
            return RedirectToAction("Index", "Content");
        }

        public IActionResult Delete(int? id)
        {
            return Forbid();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pitanje = await _context.Pitanje.FindAsync(id);

            if (pitanje == null)
            {
                return NotFound();
            }

            if (!await SmijePitanjeAsync(pitanje.Id))
            {
                return Forbid();
            }

            var odgovori = await _context.Odgovor
                .Where(o => o.PitanjeId == id)
                .ToListAsync();

            _context.Odgovor.RemoveRange(odgovori);
            _context.Pitanje.Remove(pitanje);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Pitanje je obrisano.";
            return RedirectToAction("Index", "Content");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromContent(PitanjeSaOdgovorimaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Pitanje nije sačuvano. Provjerite unesene podatke.";
                return RedirectToAction("Index", "Content");
            }

            if (!await SmijePredmetAsync(model.PredmetId) || !await SmijeOblastAsync(model.OblastId, model.PredmetId))
            {
                return Forbid();
            }

            var pitanje = new Pitanje
            {
                TekstPitanja = model.TekstPitanja,
                PredmetId = model.PredmetId,
                OblastId = model.OblastId,
                Tezina = model.Tezina
            };

            _context.Pitanje.Add(pitanje);
            await _context.SaveChangesAsync();

            var odgovori = new List<Odgovor>
            {
                new Odgovor { Tekst = model.Odgovor1, PitanjeId = pitanje.Id, IsTacan = model.TacanOdgovor == 1 },
                new Odgovor { Tekst = model.Odgovor2, PitanjeId = pitanje.Id, IsTacan = model.TacanOdgovor == 2 },
                new Odgovor { Tekst = model.Odgovor3, PitanjeId = pitanje.Id, IsTacan = model.TacanOdgovor == 3 },
                new Odgovor { Tekst = model.Odgovor4, PitanjeId = pitanje.Id, IsTacan = model.TacanOdgovor == 4 }
            };

            _context.Odgovor.AddRange(odgovori);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Pitanje je uspješno dodano.";
            return RedirectToAction("Index", "Content");
        }
    }
}
