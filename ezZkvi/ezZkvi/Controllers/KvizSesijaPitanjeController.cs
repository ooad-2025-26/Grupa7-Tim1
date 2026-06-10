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
            return Forbid();
        }

        // POST: KvizSesijaPitanje/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("ID,RedniBroj,BrojBodova,Tacno")] KvizSesijaPitanje kvizSesijaPitanje)
        {
            return Forbid();
        }

        // GET: KvizSesijaPitanje/Edit/5
        public IActionResult Edit(int? id)
        {
            return Forbid();
        }

        // POST: KvizSesijaPitanje/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("ID,RedniBroj,BrojBodova,Tacno")] KvizSesijaPitanje kvizSesijaPitanje)
        {
            return Forbid();
        }

        // GET: KvizSesijaPitanje/Delete/5
        public IActionResult Delete(int? id)
        {
            return Forbid();
        }

        // POST: KvizSesijaPitanje/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return Forbid();
        }

        private bool KvizSesijaPitanjeExists(int id)
        {
            return _context.KvizSesijaPitanja.Any(e => e.ID == id);
        }
    }
}
