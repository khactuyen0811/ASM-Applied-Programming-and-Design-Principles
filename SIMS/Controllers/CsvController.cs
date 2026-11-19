using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;

namespace SIMS.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class CsvController : Controller
    {
        private readonly ICsvTransferService _csvTransferService;

        public CsvController(ICsvTransferService csvTransferService)
        {
            _csvTransferService = csvTransferService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(IFormFile? file, CsvDatasetKind kind, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                TempData["Error"] = "Select a CSV file to preview.";
                return RedirectToAction(nameof(Index));
            }

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            CsvPreview preview = kind == CsvDatasetKind.Courses
                ? await _csvTransferService.PreviewCoursesAsync(stream, cancellationToken)
                : await _csvTransferService.PreviewStudentsAsync(stream, cancellationToken);

            ViewData["Kind"] = kind;
            ViewData["FileName"] = file.FileName;
            ViewData["FileSize"] = file.Length;
            return View(nameof(Index), preview);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile? file, CsvDatasetKind kind, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                TempData["Error"] = "Select a CSV file to import.";
                return RedirectToAction(nameof(Index));
            }

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            CsvImportResult result = kind == CsvDatasetKind.Courses
                ? await _csvTransferService.ImportCoursesAsync(stream, cancellationToken)
                : await _csvTransferService.ImportStudentsAsync(stream, cancellationToken);

            ViewData["Kind"] = kind;
            ViewData["FileName"] = file.FileName;
            ViewData["FileSize"] = file.Length;
            ViewData["Result"] = result;
            return View(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Export(CsvDatasetKind kind, CancellationToken cancellationToken)
        {
            var export = await _csvTransferService.ExportAsync(kind, cancellationToken);
            return File(export.Content, export.ContentType, export.FileName);
        }

        [HttpGet]
        public IActionResult Template(CsvDatasetKind kind)
        {
            var template = _csvTransferService.GetTemplate(kind);
            return File(template.Content, template.ContentType, template.FileName);
        }
    }
}
