using HomeBudget.Data;
using HomeBudget.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HomeBudget.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "MainPage");
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("", "Such user does not exist");
                return View(model);
            }

            if (user.Password != UserHelper.HashSHA256(model.Password))
            {
                ModelState.AddModelError("", "Invalid password.");
                return View(model);
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = model.KeepLoggedIn
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
            TempData["ToastrMessage"] = "Logged successfully";
            TempData["ToastrType"] = "success";
            return RedirectToAction("Index", "MainPage");
            
        }
    }
}
