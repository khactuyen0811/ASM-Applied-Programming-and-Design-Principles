using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Domain.Enums;

namespace SIMS.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ICourseService _courseService;

        public StudentController(
            IStudentService studentService,
            IEnrollmentService enrollmentService,
            ICourseService courseService)
        {
            _studentService = studentService;
            _enrollmentService = enrollmentService;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? program,
            StudentStatus? status,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _studentService.GetPagedAsync(search, program, status, page, pageSize, cancellationToken);

            ViewData["Search"] = search;
            ViewData["Program"] = program;
            ViewData["Status"] = status;
            ViewData["Programs"] = await _studentService.GetProgramsAsync(cancellationToken);

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var student = await _studentService.GetByIdAsync(id, cancellationToken);
            if (student is null)
            {
                return NotFound();
            }

            ViewData["AvailableCourses"] = await _courseService.GetAllAsync(cancellationToken);
            return View(student);
        }

        [HttpGet]
        [Authorize(Policy = "StaffOnly")]
        public IActionResult Create()
        {
            return View(new StudentCreateRequest { EnrollmentYear = DateTime.UtcNow.Year });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Create(StudentCreateRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _studentService.CreateAsync(request, cancellationToken);
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
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var student = await _studentService.GetByIdAsync(id, cancellationToken);
            if (student is null)
            {
                return NotFound();
            }

            return View(new StudentUpdateRequest
            {
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Email = student.Email,
                Phone = student.Phone,
                Address = student.Address,
                Program = student.Program,
                EnrollmentYear = student.EnrollmentYear,
                Status = student.Status
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Edit(int id, StudentUpdateRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _studentService.UpdateAsync(id, request, cancellationToken);
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
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _studentService.DeleteAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Enroll(int id, int courseId, CancellationToken cancellationToken)
        {
            var result = await _enrollmentService.EnrollAsync(id, courseId, cancellationToken);
            TempData["StudentMessage"] = result.Succeeded
                ? "Enrolled successfully."
                : string.Join(" ", result.Errors);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Unenroll(int id, int courseId, CancellationToken cancellationToken)
        {
            await _enrollmentService.UnenrollAsync(id, courseId, cancellationToken);
            TempData["StudentMessage"] = "Unenrolled successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
