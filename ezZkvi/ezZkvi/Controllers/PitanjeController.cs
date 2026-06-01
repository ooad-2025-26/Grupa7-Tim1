using ezZkvi.Data;
using ezZkvi.Models;
using ezZkvi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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

        // GET: Pitanje
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Pitanje.Include(p => p.Predmet);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Pitanje/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pitanje = await _context.Pitanje
                .Include(p => p.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pitanje == null)
            {
                return NotFound();
            }

            return View(pitanje);
        }

        // GET: Pitanje/Create
        public IActionResult Create()
        {
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Naziv");
            return View();
        }

        // POST: Pitanje/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PitanjeSaOdgovorimaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Naziv", model.PredmetId);
                return View(model);
            }

            var pitanje = new Pitanje
            {
                TekstPitanja = model.TekstPitanja,
                PredmetId = model.PredmetId,
                Tezina = model.Tezina
            };

            _context.Pitanje.Add(pitanje);
            await _context.SaveChangesAsync();

            _context.Odgovor.Add(new Odgovor
            {
                Tekst = model.Odgovor1,
                PitanjeId = pitanje.Id,
                IsTacan = model.TacanOdgovor == 1
            });

            _context.Odgovor.Add(new Odgovor
            {
                Tekst = model.Odgovor2,
                PitanjeId = pitanje.Id,
                IsTacan = model.TacanOdgovor == 2
            });

            _context.Odgovor.Add(new Odgovor
            {
                Tekst = model.Odgovor3,
                PitanjeId = pitanje.Id,
                IsTacan = model.TacanOdgovor == 3
            });

            _context.Odgovor.Add(new Odgovor
            {
                Tekst = model.Odgovor4,
                PitanjeId = pitanje.Id,
                IsTacan = model.TacanOdgovor == 4
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Pitanje/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pitanje = await _context.Pitanje.FindAsync(id);
            if (pitanje == null)
            {
                return NotFound();
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id", pitanje.PredmetId);
            return View(pitanje);
        }

        // POST: Pitanje/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TekstPitanja,Tezina,PredmetId")] Pitanje pitanje)
        {
            if (id != pitanje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pitanje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PitanjeExists(pitanje.Id))
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
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id", pitanje.PredmetId);
            return View(pitanje);
        }

        // GET: Pitanje/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pitanje = await _context.Pitanje
                .Include(p => p.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pitanje == null)
            {
                return NotFound();
            }

            return View(pitanje);
        }

        // POST: Pitanje/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pitanje = await _context.Pitanje.FindAsync(id);

            if (pitanje == null)
            {
                return NotFound();
            }

            var odgovori = _context.Odgovor.Where(o => o.PitanjeId == id);
            _context.Odgovor.RemoveRange(odgovori);

            _context.Pitanje.Remove(pitanje);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromContent(PitanjeSaOdgovorimaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Pitanje nije sačuvano. Provjerite unesene podatke.";
                return RedirectToAction("Content", "Moderator");
            }

            var pitanje = new Pitanje
            {
                TekstPitanja = model.TekstPitanja,
                PredmetId = model.PredmetId,
                Tezina = model.Tezina
            };

            _context.Pitanje.Add(pitanje);
            await _context.SaveChangesAsync();

            var odgovori = new List<Odgovor>
    {
        new Odgovor
        {
            Tekst = model.Odgovor1,
            PitanjeId = pitanje.Id,
            IsTacan = model.TacanOdgovor == 1
        },
        new Odgovor
        {
            Tekst = model.Odgovor2,
            PitanjeId = pitanje.Id,
            IsTacan = model.TacanOdgovor == 2
        },
        new Odgovor
        {
            Tekst = model.Odgovor3,
            PitanjeId = pitanje.Id,
            IsTacan = model.TacanOdgovor == 3
        },
        new Odgovor
        {
            Tekst = model.Odgovor4,
            PitanjeId = pitanje.Id,
            IsTacan = model.TacanOdgovor == 4
        }
    };

            _context.Odgovor.AddRange(odgovori);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Pitanje je uspješno dodano.";
            return RedirectToAction("Content", "Moderator");
        }

        private bool PitanjeExists(int id)
        {
            return _context.Pitanje.Any(e => e.Id == id);
        }
    }
}
