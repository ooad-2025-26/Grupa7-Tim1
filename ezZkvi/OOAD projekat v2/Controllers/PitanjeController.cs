using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ezZkvi.Data;
using ezZkvi.Models;

namespace OOAD_projekat_v2.Controllers
{
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
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id");
            return View();
        }

        // POST: Pitanje/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TekstPitanja,Tezina,PredmetId")] Pitanje pitanje)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pitanje);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmet, "Id", "Id", pitanje.PredmetId);
            return View(pitanje);
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
            if (pitanje != null)
            {
                _context.Pitanje.Remove(pitanje);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PitanjeExists(int id)
        {
            return _context.Pitanje.Any(e => e.Id == id);
        }
    }
}
