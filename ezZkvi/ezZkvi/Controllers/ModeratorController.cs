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

        // GET: /Moderator/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Statistike (kartice)
            ViewBag.BrojPitanja = await _context.Pitanje.CountAsync();
            ViewBag.BrojPredmeta = await _context.Predmet.CountAsync();
            ViewBag.NeobradeniFeedback = await _context.Feedback
                .CountAsync(f => f.Status == StatusFeedbacka.NA_CEKANJU);
            ViewBag.ObradeniFeedback = await _context.Feedback
                .CountAsync(f => f.Status != StatusFeedbacka.NA_CEKANJU);

            // Najnoviji feedback za obradu
            ViewBag.NajnovijiFeedback = (await _context.Feedback
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
            var pitanjaPoPredmetu = await _context.Pitanje
                .GroupBy(p => p.PredmetId)
                .Select(g => new { PredmetId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.PredmetId, x => x.Broj);

            var predmeti = await _context.Predmet.ToListAsync();

            ViewBag.PitanjaPoPredmetu = predmeti
                .Select(p => new PredmetAktivnostItem
                {
                    Naziv = p.Naziv,
                    BrojPitanja = pitanjaPoPredmetu.TryGetValue(p.Id, out var b) ? b : 0
                })
                .OrderByDescending(p => p.BrojPitanja)
                .ToList();

            // Nedavno obrađeni feedback (zamjena za log)
            ViewBag.ObradjeniFeedback = (await _context.Feedback
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
            var pitanjaQuery = _context.Pitanje
                .Include(p => p.Predmet)
                .AsQueryable();

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

            ViewData["Predmeti"] = new SelectList(_context.Predmet, "Id", "Naziv", predmetId);
            ViewData["Search"] = search;
            ViewData["Tezina"] = tezina;

            var pitanja = await pitanjaQuery
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(pitanja);
        }

        // GET: /Moderator/Feedback
        public IActionResult Feedback()
        {
            return View();
        }

        // GET: Moderator
        public async Task<IActionResult> Index()
        {
            return View(await _context.Moderator.ToListAsync());
        }

        // GET: Moderator/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moderator = await _context.Moderator
                .FirstOrDefaultAsync(m => m.Id == id);
            if (moderator == null)
            {
                return NotFound();
            }

            return View(moderator);
        }

        // GET: Moderator/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Moderator/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BrojOdgovorenihPitanja,BrojTacnihOdgovora,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Moderator moderator)
        {
            if (ModelState.IsValid)
            {
                _context.Add(moderator);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(moderator);
        }

        // GET: Moderator/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moderator = await _context.Moderator.FindAsync(id);
            if (moderator == null)
            {
                return NotFound();
            }
            return View(moderator);
        }

        // POST: Moderator/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("BrojOdgovorenihPitanja,BrojTacnihOdgovora,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Moderator moderator)
        {
            if (id != moderator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(moderator);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModeratorExists(moderator.Id))
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
            return View(moderator);
        }

        // GET: Moderator/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moderator = await _context.Moderator
                .FirstOrDefaultAsync(m => m.Id == id);
            if (moderator == null)
            {
                return NotFound();
            }

            return View(moderator);
        }

        // POST: Moderator/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var odgovori = _context.Odgovor.Where(o => o.PitanjeId == id);
            _context.Odgovor.RemoveRange(odgovori);

            var pitanje = await _context.Pitanje.FindAsync(id);

            if (pitanje != null)
            {
                _context.Pitanje.Remove(pitanje);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Content", "Moderator");
        }

        private bool ModeratorExists(string id)
        {
            return _context.Moderator.Any(e => e.Id == id);
        }
    }
}
