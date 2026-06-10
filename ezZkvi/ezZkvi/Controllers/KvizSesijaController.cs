using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ezZkvi.Data;
using ezZkvi.Models;

namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KvizSesijaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KvizSesijaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KvizSesija
        public async Task<IActionResult> Index()
        {
            return View(await _context.KvizSesije.ToListAsync());
        }

        // GET: KvizSesija/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kvizSesija = await _context.KvizSesije
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kvizSesija == null)
            {
                return NotFound();
            }

            return View(kvizSesija);
        }

        // GET: KvizSesija/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KvizSesija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,TraziBrojPitanja,VremenskoOgranicenje,Status")] KvizSesija kvizSesija)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kvizSesija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kvizSesija);
        }

        // GET: KvizSesija/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kvizSesija = await _context.KvizSesije.FindAsync(id);
            if (kvizSesija == null)
            {
                return NotFound();
            }
            return View(kvizSesija);
        }

        // POST: KvizSesija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,TraziBrojPitanja,VremenskoOgranicenje,Status")] KvizSesija kvizSesija)
        {
            if (id != kvizSesija.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kvizSesija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KvizSesijaExists(kvizSesija.ID))
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
            return View(kvizSesija);
        }

        // GET: KvizSesija/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kvizSesija = await _context.KvizSesije
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kvizSesija == null)
            {
                return NotFound();
            }

            return View(kvizSesija);
        }

        // POST: KvizSesija/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kvizSesija = await _context.KvizSesije.FindAsync(id);
            if (kvizSesija != null)
            {
                _context.KvizSesije.Remove(kvizSesija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KvizSesijaExists(int id)
        {
            return _context.KvizSesije.Any(e => e.ID == id);
        }
    }
}
