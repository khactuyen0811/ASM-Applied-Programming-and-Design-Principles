using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SIMS.Application.Abstractions;
using SIMS.Models;

namespace SIMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToDashboard(User.FindFirst(ClaimTypes.Role)?.Value);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.LoginAsync(model.Username, model.Password, cancellationToken);
            if (user is null)
            {
                ViewData["ErrorMessageLogin"] = "Your account invalid";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("DisplayName", user.DisplayName)
            };

            if (user.StudentId.HasValue)
            {
                claims.Add(new Claim("StudentId", user.StudentId.Value.ToString()));
            }

            if (user.TeacherId.HasValue)
            {
                claims.Add(new Claim("TeacherId", user.TeacherId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = model.RememberMe });

            return RedirectToDashboard(user.Role.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        private IActionResult RedirectToDashboard(string? role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (string.Equals(role, "Lecturer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Faculty", "Dashboard");
            }

            if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Student", "Dashboard");
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}

