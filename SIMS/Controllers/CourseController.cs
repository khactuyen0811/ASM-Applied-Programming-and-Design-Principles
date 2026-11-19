using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;

namespace SIMS.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ITeacherService _teacherService;
        private readonly IEnrollmentService _enrollmentService;

        public CourseController(
            ICourseService courseService,
            ITeacherService teacherService,
            IEnrollmentService enrollmentService)
        {
            _courseService = courseService;
            _teacherService = teacherService;
            _enrollmentService = enrollmentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _courseService.GetPagedAsync(search, page, pageSize, cancellationToken);
            ViewData["Search"] = search;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var course = await _courseService.GetByIdAsync(id, cancellationToken);
            if (course is null)
            {
                return NotFound();
            }

            ViewData["Enrollments"] = await _enrollmentService.GetByCourseAsync(id, cancellationToken);
            return View(course);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            ViewData["Teachers"] = await _teacherService.GetAllAsync(cancellationToken);
            return View(new CourseCreateRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(CourseCreateRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Teachers"] = await _teacherService.GetAllAsync(cancellationToken);
                return View(request);
            }

            var result = await _courseService.CreateAsync(request, cancellationToken);
            if (!result.Succeeded || result.Value is null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                ViewData["Teachers"] = await _teacherService.GetAllAsync(cancellationToken);
                return View(request);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value.Id });
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var course = await _courseService.GetByIdAsync(id, cancellationToken);
            if (course is null)
            {
                return NotFound();
            }

            ViewData["Teachers"] = await _teacherService.GetAllAsync(cancellationToken);
            return View(new CourseUpdateRequest
            {
                CourseCode = course.CourseCode,
                Name = course.Name,
                Description = course.Description,
                Credits = course.Credits,
                Capacity = course.Capacity,
                LecturerId = course.LecturerId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int id, CourseUpdateRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Teachers"] = await _teacherService.GetAllAsync(cancellationToken);
                return View(request);
            }

            var result = await _courseService.UpdateAsync(id, request, cancellationToken);
            if (!result.Succeeded || result.Value is null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                ViewData["Teachers"] = await _teacherService.GetAllAsync(cancellationToken);
                return View(request);
            }

            return RedirectToAction(nameof(Details), new { id = result.Value.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _courseService.DeleteAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
