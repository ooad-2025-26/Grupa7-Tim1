using ezZkvi.Data;
using ezZkvi.Models;
using ezZkvi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Student,Moderator,Admin")]
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? predmetId)
        {
            ViewBag.Predmeti = await _context.Predmet
                .OrderBy(p => p.Naziv)
                .Select(p => new { p.Id, p.Naziv })
                .ToListAsync();

            ViewBag.SelectedPredmetId = predmetId;
            ViewBag.BackController = GetBackController();

            var entries = await BuildLeaderboardAsync(predmetId);

            return View(new LeaderboardViewModel
            {
                Entries = entries
            });
        }

        public async Task<IActionResult> Data(int? predmetId)
        {
            var entries = await BuildLeaderboardAsync(predmetId);
            return Json(entries);
        }

        private string GetBackController()
        {
            if (User.IsInRole("Admin"))
            {
                return "Administrator";
            }

            if (User.IsInRole("Moderator"))
            {
                return "Moderator";
            }

            return "Student";
        }

        private async Task<List<LeaderboardEntryViewModel>> BuildLeaderboardAsync(int? predmetId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.KvizSesije
                .Where(s =>
                    s.Status == StatusSesije.ZAVRSEN &&
                    s.StudentId != null);

            if (predmetId.HasValue)
            {
                query = query.Where(s => s.PredmetId == predmetId.Value);
            }

            var statistika = await query
                .GroupBy(s => s.StudentId!)
                .Select(g => new
                {
                    StudentId = g.Key,
                    Kvizovi = g.Count(),
                    UkupnoTacnih = g.Sum(x => x.BrojTacnih),
                    UkupnoPitanja = g.Sum(x => x.TraziBrojPitanja)
                })
                .ToListAsync();

            var ids = statistika.Select(s => s.StudentId).ToList();

            var korisnici = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            return statistika
                .Select(s =>
                {
                    korisnici.TryGetValue(s.StudentId, out var userName);

                    var ime = !string.IsNullOrEmpty(userName) && userName.Contains('@')
                        ? userName.Split('@')[0]
                        : (userName ?? "Student");

                    var inicijali = ime.Length >= 2
                        ? ime.Substring(0, 2).ToUpper()
                        : ime.ToUpper();

                    var tacnost = s.UkupnoPitanja > 0
                        ? (int)Math.Round((double)s.UkupnoTacnih / s.UkupnoPitanja * 100)
                        : 0;

                    return new LeaderboardEntryViewModel
                    {
                        Ime = ime,
                        Inicijali = inicijali,
                        Bodovi = s.UkupnoTacnih * 10,
                        Tacnost = tacnost,
                        Kvizovi = s.Kvizovi,
                        JeTrenutni = s.StudentId == userId
                    };
                })
                .OrderByDescending(e => e.Bodovi)
                .ThenByDescending(e => e.Tacnost)
                .ToList();
        }
    }
}
