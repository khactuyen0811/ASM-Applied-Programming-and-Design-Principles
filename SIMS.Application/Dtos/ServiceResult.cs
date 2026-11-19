namespace SIMS.Application.Dtos
{
    public sealed class ServiceResult
    {
        private static readonly IReadOnlyList<string> EmptyErrors = Array.Empty<string>();

        private ServiceResult(bool succeeded, IReadOnlyList<string> errors)
        {
            Succeeded = succeeded;
            Errors = errors;
        }

        public bool Succeeded { get; }

        public IReadOnlyList<string> Errors { get; }

        public static ServiceResult Ok() => new ServiceResult(true, EmptyErrors);

        public static ServiceResult Fail(params string[] errors)
        {
            return new ServiceResult(false, errors is null || errors.Length == 0 ? EmptyErrors : errors);
        }

        public static ServiceResult Fail(IEnumerable<string> errors)
        {
            return Fail(errors is null ? Array.Empty<string>() : errors.ToArray());
        }
    }

    public sealed class ServiceResult<T>
    {
        private static readonly IReadOnlyList<string> EmptyErrors = Array.Empty<string>();

        private ServiceResult(bool succeeded, IReadOnlyList<string> errors, T? value)
        {
            Succeeded = succeeded;
            Errors = errors;
            Value = value;
        }

        public bool Succeeded { get; }

        public IReadOnlyList<string> Errors { get; }

        public T? Value { get; }

        public static ServiceResult<T> Ok(T value) => new ServiceResult<T>(true, EmptyErrors, value);

        public static ServiceResult<T> Fail(params string[] errors)
        {
            return new ServiceResult<T>(false, errors is null || errors.Length == 0 ? EmptyErrors : errors, default);
        }

        public static ServiceResult<T> Fail(IEnumerable<string> errors)
        {
            return Fail(errors is null ? Array.Empty<string>() : errors.ToArray());
        }
    }
}
