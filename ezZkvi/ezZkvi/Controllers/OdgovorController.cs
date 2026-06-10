using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ezZkvi.Models;

namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class OdgovorController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Content");
        }

        public IActionResult Details(int? id)
        {
            return Forbid();
        }

        public IActionResult Create()
        {
            return Forbid();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Tekst,IsTacan,PitanjeId")] Odgovor odgovor)
        {
            return Forbid();
        }

        public IActionResult Edit(int? id)
        {
            return Forbid();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Tekst,IsTacan,PitanjeId")] Odgovor odgovor)
        {
            return Forbid();
        }

        public IActionResult Delete(int? id)
        {
            return Forbid();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return Forbid();
        }
    }
}
