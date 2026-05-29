using ezZkvi.Models;
using ezZkvi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ezZkvi.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<Korisnik> _signInManager;
        private readonly UserManager<Korisnik> _userManager;

        public AccountController(SignInManager<Korisnik> signInManager,
                                 UserManager<Korisnik> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Pogrešan email ili lozinka.");
                return View(model);
            }

            if (!user.IsApproved)
            {
                ModelState.AddModelError("", "Vaš nalog još nije odobren od strane administratora.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Dashboard", "Administrator");

                if (await _userManager.IsInRoleAsync(user, "Moderator"))
                    return RedirectToAction("Dashboard", "Moderator");

                if (await _userManager.IsInRoleAsync(user, "Student"))
                    return RedirectToAction("Dashboard", "Student");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Pogrešan email ili lozinka.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return RedirectToAction("Login");
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewData["ActiveTab"] = "register";

            if (!ModelState.IsValid)
                return View("Login", model);

            var baseUsername = model.Email.Split('@')[0];
            var username = baseUsername;
            int suffix = 1;
            while (await _userManager.FindByNameAsync(username) != null)
            {
                username = baseUsername + suffix;
                suffix++;
            }

            var user = new Korisnik
            {
                UserName = username,
                Email = model.Email,
                EmailConfirmed = true,
                IsApproved = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");

                TempData["Message"] = $"Registracija je uspješna. Vaš username je '{username}'. Nalog čeka odobrenje administratora.";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Login", model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}