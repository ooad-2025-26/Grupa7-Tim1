using ezZkvi.Data;
using ezZkvi.Models;
using ezZkvi.Services;
using ezZkvi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ezZkvi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministratorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        private readonly IEmailService _emailService;

        public AdministratorController(
            ApplicationDbContext context,
            UserManager<Korisnik> userManager,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: /Administrator/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
            .Where(u => u.EmailConfirmed)
            .ToListAsync();

            var model = new List<AdminUserViewModel>();
            var aktivnostPrag = DateTime.UtcNow.AddMinutes(-5);

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "Nema ulogu",
                    IsApproved = user.IsApproved,
                    IsActive = user.IsApproved && user.LastActivity.HasValue && user.LastActivity.Value >= aktivnostPrag
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (!user.EmailConfirmed)
            {
                TempData["Error"] = "Korisnik ne može biti odobren dok ne potvrdi email adresu.";
                return RedirectToAction(nameof(Users));
            }

            user.IsApproved = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Greška prilikom odobravanja korisnika.";
                return RedirectToAction(nameof(Users));
            }

            if (!await _userManager.IsInRoleAsync(user, "Student"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Student");

                if (!roleResult.Succeeded)
                {
                    TempData["Error"] = "Korisnik je odobren, ali uloga nije dodijeljena.";
                    return RedirectToAction(nameof(Users));
                }
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    var loginUrl = Url.Action("Login", "Account", null, Request.Scheme);

                    var subject = "Vaš eZkvi nalog je odobren";

                    var body = $@"
                            Poštovani,

                            Vaš korisnički nalog na eZkvi platformi je odobren.

                            Sada se možete prijaviti u sistem koristeći svoju email adresu:
                            {user.Email}
                            ";

                    await _emailService.SendEmailAsync(
                        user.Email,
                        subject,
                        body
                    );

                    TempData["Message"] = $"Korisnik {user.Email} je odobren i poslan mu je email.";
                }
                catch
                {
                    TempData["Message"] = $"Korisnik {user.Email} je odobren, ali email obavijest nije poslana.";
                }
            }
            else
            {
                TempData["Message"] = "Korisnik je odobren, ali nema registrovanu email adresu.";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (user.IsApproved)
                return RedirectToAction(nameof(Users));

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAccess(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["Error"] = "Ne mo�ete ukloniti pristup sami sebi.";
                return RedirectToAction(nameof(Users));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Ne mo�ete ukloniti pristup administratoru.";
                return RedirectToAction(nameof(Users));
            }

            user.IsApproved = false;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Gre�ka prilikom uklanjanja pristupa.";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.UpdateSecurityStampAsync(user);

            TempData["Message"] = "Korisniku je uklonjen pristup.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string id, string role)
        {
            var allowedRoles = new[] { "Admin", "Moderator", "Student" };

            if (!allowedRoles.Contains(role))
            {
                TempData["Error"] = "Neispravna uloga.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (!user.EmailConfirmed)
            {
                TempData["Error"] = "Korisniku se ne može mijenjati uloga dok ne potvrdi email adresu.";
                return RedirectToAction(nameof(Users));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["Error"] = "Ne mo�ete mijenjati vlastitu ulogu.";
                return RedirectToAction(nameof(Users));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = "Gre�ka prilikom uklanjanja stare uloge.";
                    return RedirectToAction(nameof(Users));
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, role);

            if (!addResult.Succeeded)
            {
                TempData["Error"] = "Gre�ka prilikom dodjeljivanja nove uloge.";
                return RedirectToAction(nameof(Users));
            }

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);
            await _userManager.UpdateSecurityStampAsync(user);

            TempData["Message"] = $"Korisniku {user.Email} dodijeljena je uloga {role}.";
            return RedirectToAction(nameof(Users));
        }

        // GET: /Administrator/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Statistike (kartice)
            ViewBag.BrojKorisnika = await _context.Users.CountAsync(u => u.EmailConfirmed);
            ViewBag.BrojPitanja = await _context.Pitanje.CountAsync();
            ViewBag.BrojKvizova = await _context.KvizSesije.CountAsync();
            ViewBag.NeobradeniFeedback = await _context.Feedback
                .CountAsync(f => f.Status == StatusFeedbacka.NA_CEKANJU);

            // Na čekanju – neodobreni korisnici
            var neodobreni = await _context.Users
               .Where(u => u.EmailConfirmed && !u.IsApproved)
               .ToListAsync();

            ViewBag.NaCekanju = neodobreni.Select(u =>
            {
                var ime = !string.IsNullOrEmpty(u.UserName) && u.UserName.Contains('@')
                    ? u.UserName.Split('@')[0]
                    : (u.UserName ?? "Korisnik");
                return new KorisnikNaCekanjuItem
                {
                    Id = u.Id,
                    Ime = ime,
                    Inicijali = ime.Length >= 2 ? ime.Substring(0, 2).ToUpper() : ime.ToUpper()
                };
            }).ToList();

            // Najaktivniji predmeti – po broju odrađenih kvizova
            var kvizoviPoPredmetu = await _context.KvizSesije
                .Where(s => s.PredmetId != null)
                .GroupBy(s => s.PredmetId!.Value)
                .Select(g => new { PredmetId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.PredmetId, x => x.Broj);

            var pitanjaPoPredmetu = await _context.Pitanje
                .GroupBy(p => p.PredmetId)
                .Select(g => new { PredmetId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.PredmetId, x => x.Broj);

            var predmeti = await _context.Predmet.ToListAsync();

            ViewBag.NajaktivnijiPredmeti = predmeti
                .Select(p => new PredmetAktivnostItem
                {
                    Naziv = p.Naziv,
                    BrojPitanja = pitanjaPoPredmetu.TryGetValue(p.Id, out var bp) ? bp : 0,
                    BrojKvizova = kvizoviPoPredmetu.TryGetValue(p.Id, out var bk) ? bk : 0
                })
                .OrderByDescending(p => p.BrojKvizova)
                .Take(5)
                .ToList();

            // Nedavna aktivnost – zadnji završeni kvizovi
            var nedavni = await _context.KvizSesije
                .Include(s => s.Predmet)
                .Where(s => s.Status == StatusSesije.ZAVRSEN)
                .OrderByDescending(s => s.DatumZavrsetka)
                .Take(5)
                .ToListAsync();

            var ids = nedavni.Where(s => s.StudentId != null).Select(s => s.StudentId!).Distinct().ToList();
            var imena = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            ViewBag.NedavniKvizovi = nedavni.Select(s =>
            {
                var userName = s.StudentId != null && imena.TryGetValue(s.StudentId, out var n) ? n : null;
                var ime = !string.IsNullOrEmpty(userName) && userName.Contains('@')
                    ? userName.Split('@')[0]
                    : (userName ?? "Nepoznat");
                return new KvizAktivnostItem
                {
                    Korisnik = ime,
                    Predmet = s.Predmet != null ? s.Predmet.Naziv : "—",
                    Procenat = s.Procenat,
                    Datum = s.DatumZavrsetka
                };
            }).ToList();

            return View();
        }

        // GET: Administrator
        public async Task<IActionResult> Index()
        {
            return View(await _context.Administrator.ToListAsync());
        }

        // GET: Administrator/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrator = await _context.Administrator
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrator == null)
            {
                return NotFound();
            }

            return View(administrator);
        }

        // GET: Administrator/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Administrator/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Administrator administrator)
        {
            if (ModelState.IsValid)
            {
                _context.Add(administrator);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(administrator);
        }

        // GET: Administrator/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrator = await _context.Administrator.FindAsync(id);
            if (administrator == null)
            {
                return NotFound();
            }
            return View(administrator);
        }

        // POST: Administrator/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Administrator administrator)
        {
            if (id != administrator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(administrator);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdministratorExists(administrator.Id))
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
            return View(administrator);
        }

        // GET: Administrator/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrator = await _context.Administrator
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrator == null)
            {
                return NotFound();
            }

            return View(administrator);
        }

        // POST: Administrator/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var administrator = await _context.Administrator.FindAsync(id);
            if (administrator != null)
            {
                _context.Administrator.Remove(administrator);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdministratorExists(string id)
        {
            return _context.Administrator.Any(e => e.Id == id);
        }

        // GET: /Administrator/EmailObavijest
        public IActionResult EmailObavijest()
        {
            return View(new AdminEmailObavijestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailObavijest(AdminEmailObavijestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            List<Korisnik> korisnici;

            if (model.Primaoci == "Studenti")
            {
                korisnici = (await _userManager.GetUsersInRoleAsync("Student"))
                    .Where(u => u.Email != null && u.IsApproved)
                    .ToList();
            }
            else if (model.Primaoci == "AdminiModeratori")
            {
                var admini = await _userManager.GetUsersInRoleAsync("Admin");
                var moderatori = await _userManager.GetUsersInRoleAsync("Moderator");

                korisnici = admini
                    .Concat(moderatori)
                    .Where(u => u.Email != null && u.IsApproved)
                    .DistinctBy(u => u.Id)
                    .ToList();
            }
            else
            {
                TempData["Error"] = "Odabrana grupa primalaca nije validna.";
                return RedirectToAction(nameof(EmailObavijest));
            }

            if (!korisnici.Any())
            {
                TempData["Error"] = "Nema korisnika za odabranu grupu primalaca.";
                return RedirectToAction(nameof(EmailObavijest));
            }

            try
            {
                foreach (var korisnik in korisnici)
                {
                    await _emailService.SendEmailAsync(
                        korisnik.Email!,
                        model.Naslov,
                        model.Poruka
                    );
                }
            }
            catch (InvalidOperationException ex)
            {
                // PRIVREMENO: prikaži pravi razlog (inner exception) radi dijagnostike
                TempData["Error"] = ex.Message + " | DETALJ: " + (ex.InnerException?.Message ?? "nema detalja");
                return RedirectToAction(nameof(EmailObavijest));
            }
            catch
            {
                TempData["Error"] = "Došlo je do greške prilikom slanja email obavijesti.";
                return RedirectToAction(nameof(EmailObavijest));
            }

            string grupa = model.Primaoci == "Studenti"
                ? "studentima"
                : "adminima i moderatorima";

            TempData["Message"] = $"Email obavijest je poslana {grupa}. Broj primalaca: {korisnici.Count}";

            return RedirectToAction(nameof(EmailObavijest));
        }
    }
}
