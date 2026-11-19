using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Domain.Enums;

namespace SIMS.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            UserRole? role,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var users = await _userService.GetPagedAsync(search, role, page, pageSize, cancellationToken);
            ViewData["Search"] = search;
            ViewData["Role"] = role;
            ViewData["Summary"] = await _userService.GetSummaryAsync(cancellationToken);
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int userId, UserRole role, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateRoleAsync(new UpdateUserRoleRequest(userId, role), cancellationToken);
            SetResultMessage(result, "Role updated successfully.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(int userId, bool active, CancellationToken cancellationToken)
        {
            var result = await _userService.SetActiveAsync(userId, active, cancellationToken);
            SetResultMessage(result, active ? "Account activated." : "Account suspended.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int userId, CancellationToken cancellationToken)
        {
            var result = await _userService.ResetPasswordAsync(userId, cancellationToken);
            if (result.Succeeded && result.Value is not null)
            {
                TempData["Success"] = $"Temporary password for {result.Value.Username}: {result.Value.TemporaryPassword}";
            }
            else
            {
                TempData["Error"] = string.Join(" ", result.Errors);
            }

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
    }
}
