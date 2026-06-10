using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ezZkvi.Data;
using ezZkvi.Models;

namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KvizSesijaPitanjeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KvizSesijaPitanjeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KvizSesijaPitanje
        public async Task<IActionResult> Index()
        {
            return View(await _context.KvizSesijaPitanja.ToListAsync());
        }

        // GET: KvizSesijaPitanje/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kvizSesijaPitanje = await _context.KvizSesijaPitanja
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kvizSesijaPitanje == null)
            {
                return NotFound();
            }

            return View(kvizSesijaPitanje);
        }

        // GET: KvizSesijaPitanje/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KvizSesijaPitanje/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,RedniBroj,BrojBodova,Tacno")] KvizSesijaPitanje kvizSesijaPitanje)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kvizSesijaPitanje);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kvizSesijaPitanje);
        }

        // GET: KvizSesijaPitanje/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kvizSesijaPitanje = await _context.KvizSesijaPitanja.FindAsync(id);
            if (kvizSesijaPitanje == null)
            {
                return NotFound();
            }
            return View(kvizSesijaPitanje);
        }

        // POST: KvizSesijaPitanje/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,RedniBroj,BrojBodova,Tacno")] KvizSesijaPitanje kvizSesijaPitanje)
        {
            if (id != kvizSesijaPitanje.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kvizSesijaPitanje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KvizSesijaPitanjeExists(kvizSesijaPitanje.ID))
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
            return View(kvizSesijaPitanje);
        }

        // GET: KvizSesijaPitanje/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kvizSesijaPitanje = await _context.KvizSesijaPitanja
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kvizSesijaPitanje == null)
            {
                return NotFound();
            }

            return View(kvizSesijaPitanje);
        }

        // POST: KvizSesijaPitanje/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kvizSesijaPitanje = await _context.KvizSesijaPitanja.FindAsync(id);
            if (kvizSesijaPitanje != null)
            {
                _context.KvizSesijaPitanja.Remove(kvizSesijaPitanje);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KvizSesijaPitanjeExists(int id)
        {
            return _context.KvizSesijaPitanja.Any(e => e.ID == id);
        }
    }
}
