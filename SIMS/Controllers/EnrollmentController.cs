using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;

namespace SIMS.Controllers
{
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ICourseService _courseService;
        private readonly IStudentService _studentService;

        public EnrollmentController(
            IEnrollmentService enrollmentService,
            ICourseService courseService,
            IStudentService studentService)
        {
            _enrollmentService = enrollmentService;
            _courseService = courseService;
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int? studentId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var isStudent = User.IsInRole("Student");
            if (isStudent)
            {
                if (!TryGetClaimId("StudentId", out var ownStudentId))
                {
                    return Forbid();
                }

                studentId = ownStudentId;
            }

            IReadOnlyList<EnrollmentListItem> enrollments = Array.Empty<EnrollmentListItem>();
            StudentDetails? selectedStudent = null;

            if (studentId.HasValue)
            {
                selectedStudent = await _studentService.GetByIdAsync(studentId.Value, cancellationToken);
                if (selectedStudent is not null)
                {
                    enrollments = await _enrollmentService.GetByStudentAsync(studentId.Value, cancellationToken);
                }
            }

            if (!isStudent)
            {
                var students = await _studentService.GetPagedAsync(search, null, null, page, pageSize, cancellationToken);
                ViewData["Students"] = students;
            }

            ViewData["SelectedStudent"] = selectedStudent;
            ViewData["SelectedStudentId"] = studentId;
            ViewData["Courses"] = await _courseService.GetAllAsync(cancellationToken);
            ViewData["Search"] = search;

            return View(enrollments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int studentId, int courseId, string? search, CancellationToken cancellationToken)
        {
            studentId = ResolveStudentId(studentId);
            if (studentId <= 0)
            {
                return Forbid();
            }

            var result = await _enrollmentService.EnrollAsync(studentId, courseId, cancellationToken);
            if (result.Succeeded)
            {
                TempData["Success"] = "Student enrolled successfully.";
            }
            else
            {
                TempData["Error"] = string.Join(" ", result.Errors);
            }

            return RedirectToAction(nameof(Index), new { studentId, search });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int studentId, int courseId, string? search, CancellationToken cancellationToken)
        {
            studentId = ResolveStudentId(studentId);
            if (studentId <= 0)
            {
                return Forbid();
            }

            await _enrollmentService.UnenrollAsync(studentId, courseId, cancellationToken);
            TempData["Success"] = "Enrollment removed successfully.";

            return RedirectToAction(nameof(Index), new { studentId, search });
        }

        private int ResolveStudentId(int requestedStudentId)
        {
            if (User.IsInRole("Student"))
            {
                return TryGetClaimId("StudentId", out var ownStudentId) ? ownStudentId : 0;
            }

            return requestedStudentId;
        }

        private bool TryGetClaimId(string claimType, out int id)
        {
            return int.TryParse(User.FindFirst(claimType)?.Value, out id);
        }
    }
}
