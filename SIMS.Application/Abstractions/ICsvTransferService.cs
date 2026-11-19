using SIMS.Application.Dtos;

namespace SIMS.Application.Abstractions
{
    public interface ICsvTransferService
    {
        Task<CsvPreview> PreviewStudentsAsync(Stream stream, CancellationToken cancellationToken = default);

        Task<CsvImportResult> ImportStudentsAsync(Stream stream, CancellationToken cancellationToken = default);

        Task<CsvPreview> PreviewCoursesAsync(Stream stream, CancellationToken cancellationToken = default);

        Task<CsvImportResult> ImportCoursesAsync(Stream stream, CancellationToken cancellationToken = default);

        Task<CsvExportFile> ExportAsync(CsvDatasetKind kind, CancellationToken cancellationToken = default);

        CsvExportFile GetTemplate(CsvDatasetKind kind);
    }
}
