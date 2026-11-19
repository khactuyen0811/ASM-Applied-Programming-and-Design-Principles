using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;

namespace SIMS.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class TeacherController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly ICourseService _courseService;

        public TeacherController(ITeacherService teacherService, ICourseService courseService)
        {
            _teacherService = teacherService;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _teacherService.GetPagedAsync(search, page, pageSize, cancellationToken);
            ViewData["Search"] = search;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var teacher = await _teacherService.GetByIdAsync(id, cancellationToken);
            if (teacher is null)
            {
                return NotFound();
            }

            ViewData["Courses"] = await _courseService.GetByLecturerAsync(id, cancellationToken);
            return View(teacher);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TeacherCreateRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherCreateRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _teacherService.CreateAsync(request, cancellationToken);
            if (!result.Succeeded || result.Value is null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(request);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var teacher = await _teacherService.GetByIdAsync(id, cancellationToken);
            if (teacher is null)
            {
                return NotFound();
            }

            return View(new TeacherUpdateRequest
            {
                TeacherCode = teacher.TeacherCode,
                FullName = teacher.FullName,
                Email = teacher.Email,
                Phone = teacher.Phone,
                Department = teacher.Department
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherUpdateRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _teacherService.UpdateAsync(id, request, cancellationToken);
            if (!result.Succeeded || result.Value is null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(request);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _teacherService.DeleteAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
