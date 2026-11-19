using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;

namespace SIMS.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;

        public ProfileController(
            IUserService userService,
            IStudentService studentService,
            ITeacherService teacherService)
        {
            _userService = userService;
            _studentService = studentService;
            _teacherService = teacherService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return NotFound();
            }

            if (user.StudentId.HasValue)
            {
                ViewData["Student"] = await _studentService.GetByIdAsync(user.StudentId.Value, cancellationToken);
            }

            if (user.TeacherId.HasValue)
            {
                ViewData["Teacher"] = await _teacherService.GetByIdAsync(user.TeacherId.Value, cancellationToken);
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDisplayName(string displayName, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Forbid();
            }

            var result = await _userService.UpdateDisplayNameAsync(userId, displayName, cancellationToken);
            SetResultMessage(result, "Display name updated.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userService.ChangePasswordAsync(
                new ChangePasswordRequest(userId, currentPassword ?? string.Empty, newPassword),
                cancellationToken);

            SetResultMessage(result, "Password changed successfully.");
            return RedirectToAction(nameof(Index));
        }

        private void SetResultMessage(ServiceResult result, string successMessage)
        {
            if (result.Succeeded)
            {
                TempData["Success"] = successMessage;
            }
            else
            {
                TempData["Error"] = string.Join(" ", result.Errors);
            }
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);
        }
    }
}
