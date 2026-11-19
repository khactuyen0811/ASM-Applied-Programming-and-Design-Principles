namespace SIMS.Domain.Common
{
    public sealed class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page < 1 ? 1 : page;
            PageSize = pageSize < 1 ? 10 : pageSize;
            TotalCount = totalCount < 0 ? 0 : totalCount;
        }

        public IReadOnlyList<T> Items { get; }

        public int Page { get; }

        public int PageSize { get; }

        public int TotalCount { get; }

        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPrevious => Page > 1;

        public bool HasNext => Page < TotalPages;

        public static PagedResult<T> Empty(int page, int pageSize) => new(Array.Empty<T>(), page, pageSize, 0);
    }
}
