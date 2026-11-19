namespace SIMS.Domain.Abstractions
{
    public interface IRangeRepository<T>
        where T : class
    {
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    }
}
