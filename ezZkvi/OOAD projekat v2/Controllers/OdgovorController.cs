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
    public class OdgovorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OdgovorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Odgovor
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Odgovor.Include(o => o.Pitanje);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Odgovor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odgovor = await _context.Odgovor
                .Include(o => o.Pitanje)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (odgovor == null)
            {
                return NotFound();
            }

            return View(odgovor);
        }

        // GET: Odgovor/Create
        public IActionResult Create()
        {
            ViewData["PitanjeId"] = new SelectList(_context.Pitanje, "Id", "Id");
            return View();
        }

        // POST: Odgovor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tekst,IsTacan,PitanjeId")] Odgovor odgovor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(odgovor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PitanjeId"] = new SelectList(_context.Pitanje, "Id", "Id", odgovor.PitanjeId);
            return View(odgovor);
        }

        // GET: Odgovor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odgovor = await _context.Odgovor.FindAsync(id);
            if (odgovor == null)
            {
                return NotFound();
            }
            ViewData["PitanjeId"] = new SelectList(_context.Pitanje, "Id", "Id", odgovor.PitanjeId);
            return View(odgovor);
        }

        // POST: Odgovor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tekst,IsTacan,PitanjeId")] Odgovor odgovor)
        {
            if (id != odgovor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(odgovor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OdgovorExists(odgovor.Id))
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
            ViewData["PitanjeId"] = new SelectList(_context.Pitanje, "Id", "Id", odgovor.PitanjeId);
            return View(odgovor);
        }

        // GET: Odgovor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odgovor = await _context.Odgovor
                .Include(o => o.Pitanje)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (odgovor == null)
            {
                return NotFound();
            }

            return View(odgovor);
        }

        // POST: Odgovor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var odgovor = await _context.Odgovor.FindAsync(id);
            if (odgovor != null)
            {
                _context.Odgovor.Remove(odgovor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OdgovorExists(int id)
        {
            return _context.Odgovor.Any(e => e.Id == id);
        }
    }
}
