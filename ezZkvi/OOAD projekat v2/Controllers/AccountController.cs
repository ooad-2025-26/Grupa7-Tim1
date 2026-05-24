using Microsoft.AspNetCore.Mvc;

namespace ezZkvi.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }
    }
}
