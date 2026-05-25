using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ezZkvi.Data;
using ezZkvi.Models;

namespace ezZkvi.Controllers
{
    public class ModeratorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModeratorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Moderator/Content
        public IActionResult Content()
        {
            return View();
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var moderator = await _context.Moderator.FindAsync(id);
            if (moderator != null)
            {
                _context.Moderator.Remove(moderator);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ModeratorExists(string id)
        {
            return _context.Moderator.Any(e => e.Id == id);
        }
    }
}
