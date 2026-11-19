namespace SIMS.Application.Dtos
{
    public enum CsvDatasetKind
    {
        Students = 1,
        Courses = 2,
        Enrollments = 3
    }

    public sealed class CsvPreviewRow
    {
        public CsvPreviewRow()
        {
        }

        public CsvPreviewRow(int rowNumber, IReadOnlyList<string> values, string? error)
        {
            RowNumber = rowNumber;
            Values = values ?? Array.Empty<string>();
            Error = error;
        }

        public int RowNumber { get; set; }

        public IReadOnlyList<string> Values { get; set; } = Array.Empty<string>();

        public string? Error { get; set; }
    }

    public sealed class CsvPreview
    {
        public CsvPreview()
        {
        }

        public CsvPreview(
            CsvDatasetKind kind,
            IReadOnlyList<string> headers,
            IReadOnlyList<CsvPreviewRow> rows,
            int totalRows,
            IReadOnlyList<string> errors,
            bool isValid)
        {
            Kind = kind;
            Headers = headers ?? Array.Empty<string>();
            Rows = rows ?? Array.Empty<CsvPreviewRow>();
            TotalRows = totalRows;
            Errors = errors ?? Array.Empty<string>();
            IsValid = isValid;
        }

        public CsvDatasetKind Kind { get; set; }

        public IReadOnlyList<string> Headers { get; set; } = Array.Empty<string>();

        public IReadOnlyList<CsvPreviewRow> Rows { get; set; } = Array.Empty<CsvPreviewRow>();

        public int TotalRows { get; set; }

        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

        public bool IsValid { get; set; }
    }

    public sealed class CsvImportError
    {
        public CsvImportError()
        {
        }

        public CsvImportError(int rowNumber, string message)
        {
            RowNumber = rowNumber;
            Message = message;
        }

        public int RowNumber { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    public sealed class CsvImportResult
    {
        public CsvImportResult()
        {
        }

        public CsvImportResult(
            CsvDatasetKind kind,
            int total,
            int imported,
            int failed,
            IReadOnlyList<CsvImportError> errors,
            bool succeeded)
        {
            Kind = kind;
            Total = total;
            Imported = imported;
            Failed = failed;
            Errors = errors ?? Array.Empty<CsvImportError>();
            Succeeded = succeeded;
        }

        public CsvDatasetKind Kind { get; set; }

        public int Total { get; set; }

        public int Imported { get; set; }

        public int Failed { get; set; }

        public IReadOnlyList<CsvImportError> Errors { get; set; } = Array.Empty<CsvImportError>();

        public bool Succeeded { get; set; }
    }

    public sealed class CsvExportFile
    {
        public CsvExportFile()
        {
        }

        public CsvExportFile(string fileName, string contentType, byte[] content)
        {
            FileName = fileName;
            ContentType = contentType;
            Content = content ?? Array.Empty<byte>();
        }

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public byte[] Content { get; set; } = Array.Empty<byte>();
    }

    public sealed class CsvTransferOptions
    {
        public int MaxPreviewRows { get; set; } = 10;

        public int MaxRows { get; set; } = 20000;
    }
}
