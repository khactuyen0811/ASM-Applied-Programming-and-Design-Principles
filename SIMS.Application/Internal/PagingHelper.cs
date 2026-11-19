namespace SIMS.Application.Internal
{
    internal static class PagingHelper
    {
        public const int DefaultPageSize = 20;

        public const int MaxPageSize = 200;

        public static (int Page, int PageSize) Normalize(int page, int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = DefaultPageSize;
            }

            if (pageSize > MaxPageSize)
            {
                pageSize = MaxPageSize;
            }

            return (page, pageSize);
        }
    }
}
