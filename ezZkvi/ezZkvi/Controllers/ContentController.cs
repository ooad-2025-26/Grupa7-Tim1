using System.Security.Claims;
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
    [Route("[controller]/[action]")]
    public class ContentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<List<int>?> DozvoljeniPredmetiIdAsync()
        {
            if (User.IsInRole("Admin"))
            {
                return null;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Predmet
                .Where(p => p.KreatorId == userId)
                .Select(p => p.Id)
                .ToListAsync();
        }

        [HttpGet("/Content")]
        public IActionResult Root()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? predmetId, int? oblastId, Tezina? tezina)
        {
            var dozvoljeni = await DozvoljeniPredmetiIdAsync();

            var pitanjaQuery = _context.Pitanje
                .Include(p => p.Predmet)
                .Include(p => p.Oblast)
                .AsQueryable();

            if (dozvoljeni != null)
            {
                pitanjaQuery = pitanjaQuery.Where(p => dozvoljeni.Contains(p.PredmetId));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                pitanjaQuery = pitanjaQuery.Where(p => p.TekstPitanja.Contains(search));
            }

            if (predmetId.HasValue)
            {
                pitanjaQuery = pitanjaQuery.Where(p => p.PredmetId == predmetId.Value);
            }

            if (tezina.HasValue)
            {
                pitanjaQuery = pitanjaQuery.Where(p => p.Tezina == tezina.Value);
            }

            var predmetiQuery = _context.Predmet.AsQueryable();
            if (dozvoljeni != null)
            {
                predmetiQuery = predmetiQuery.Where(p => dozvoljeni.Contains(p.Id));
            }

            var predmeti = await predmetiQuery.OrderBy(p => p.Naziv).ToListAsync();
            var predmetiIds = predmeti.Select(p => p.Id).ToList();

            var oblasti = await _context.Oblast
                .Include(o => o.Predmet)
                .Where(o => predmetiIds.Contains(o.PredmetId))
                .OrderBy(o => o.Predmet!.Naziv)
                .ThenBy(o => o.Naziv)
                .ToListAsync();

            if (predmetId.HasValue && oblastId.HasValue)
            {
                var oblastPripadaPredmetu = oblasti.Any(o =>
                    o.Id == oblastId.Value &&
                    o.PredmetId == predmetId.Value);

                if (!oblastPripadaPredmetu)
                {
                    oblastId = null;
                }
            }

            var oblastiZaPrikaz = predmetId.HasValue
                ? oblasti.Where(o => o.PredmetId == predmetId.Value).ToList()
                : oblasti;

            if (oblastId.HasValue)
            {
                pitanjaQuery = pitanjaQuery.Where(p => p.OblastId == oblastId.Value);
            }

            var oblastiSelect = oblastiZaPrikaz
                .Select(o => new
                {
                    o.Id,
                    Naziv = (o.Predmet != null ? o.Predmet.Naziv : "Predmet") + " / " + o.Naziv
                })
                .ToList();

            ViewData["Predmeti"] = new SelectList(predmeti, "Id", "Naziv", predmetId);
            ViewData["OblastiFilter"] = new SelectList(oblastiSelect, "Id", "Naziv", oblastId);
            ViewData["Search"] = search;
            ViewData["Tezina"] = tezina;
            ViewData["OblastId"] = oblastId;

            ViewBag.OblastiListaZaSelect = oblasti.Select(o => new
            {
                o.Id,
                o.Naziv,
                o.PredmetId,
                PredmetNaziv = o.Predmet != null ? o.Predmet.Naziv : "Predmet"
            }).ToList();

            var brojPitanjaPoPredmetu = await _context.Pitanje
                .Where(p => predmetiIds.Contains(p.PredmetId))
                .GroupBy(p => p.PredmetId)
                .Select(g => new { PredmetId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.PredmetId, x => x.Broj);

            ViewBag.PredmetiLista = predmeti
                .Select(p => new PredmetAktivnostItem
                {
                    Id = p.Id,
                    Naziv = p.Naziv,
                    BrojPitanja = brojPitanjaPoPredmetu.TryGetValue(p.Id, out var b) ? b : 0
                })
                .ToList();

            var brojPitanjaPoOblasti = await _context.Pitanje
                .Where(p => predmetiIds.Contains(p.PredmetId))
                .GroupBy(p => p.OblastId)
                .Select(g => new { OblastId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.OblastId, x => x.Broj);

            ViewBag.OblastiLista = oblastiZaPrikaz
                .Select(o => new OblastAktivnostItem
                {
                    Id = o.Id,
                    Naziv = o.Naziv,
                    PredmetId = o.PredmetId,
                    PredmetNaziv = o.Predmet != null ? o.Predmet.Naziv : "Predmet",
                    BrojPitanja = brojPitanjaPoOblasti.TryGetValue(o.Id, out var b) ? b : 0
                })
                .ToList();

            var pitanja = await pitanjaQuery
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var pitanjaIds = pitanja.Select(p => p.Id).ToList();
            var odgovori = await _context.Odgovor
                .Where(o => pitanjaIds.Contains(o.PitanjeId))
                .OrderBy(o => o.Id)
                .ToListAsync();

            ViewBag.OdgovoriPoPitanju = odgovori
                .GroupBy(o => o.PitanjeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(pitanja);
        }
    }
}
