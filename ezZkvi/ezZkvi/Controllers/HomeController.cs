using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ezZkvi.Data;
using ezZkvi.Models;

namespace ezZkvi.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.BrojKorisnika = await _context.Users.CountAsync();
            ViewBag.BrojPitanja = await _context.Pitanje.CountAsync();
            ViewBag.BrojKvizova = await _context.KvizSesije.CountAsync();
            ViewBag.BrojPredmeta = await _context.Predmet.CountAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Prikazuje se za HTTP greške (npr. 404 - stranica ne postoji)
        public IActionResult Greska(int? kod)
        {
            ViewBag.Kod = kod;
            return View();
        }
    }
}
