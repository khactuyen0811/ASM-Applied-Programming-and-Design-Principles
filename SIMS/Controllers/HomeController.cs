using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Models;

namespace SIMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Index", "Login");
            }

            return RedirectToDashboard();
        }

        [ActionName("NotFound")]
        public IActionResult NotFoundPage()
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View();
        }

        public IActionResult Denied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult States()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (User.IsInRole("Lecturer"))
            {
                return RedirectToAction("Faculty", "Dashboard");
            }

            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Student", "Dashboard");
            }

            return RedirectToAction("Index", "Login");
        }
    }
}
