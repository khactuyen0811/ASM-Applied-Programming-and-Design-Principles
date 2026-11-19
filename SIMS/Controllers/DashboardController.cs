using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;

namespace SIMS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var stats = await _dashboardService.GetStatsAsync(cancellationToken);
            var trend = await _dashboardService.GetEnrollmentTrendAsync(6, cancellationToken);
            ViewData["Trend"] = trend;
            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> Faculty(CancellationToken cancellationToken)
        {
            if (!TryGetClaimId("TeacherId", out var teacherId))
            {
                return Forbid();
            }

            var stats = await _dashboardService.GetFacultyAsync(teacherId, cancellationToken);
            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> Student(CancellationToken cancellationToken)
        {
            if (!TryGetClaimId("StudentId", out var studentId))
            {
                return Forbid();
            }

            var stats = await _dashboardService.GetStudentAsync(studentId, cancellationToken);
            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> Stats(CancellationToken cancellationToken)
        {
            var stats = await _dashboardService.GetStatsAsync(cancellationToken);
            return Json(stats);
        }

        private bool TryGetClaimId(string claimType, out int id)
        {
            return int.TryParse(User.FindFirst(claimType)?.Value, out id);
        }
    }
}
