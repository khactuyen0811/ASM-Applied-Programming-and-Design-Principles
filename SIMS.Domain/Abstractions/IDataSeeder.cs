namespace SIMS.Domain.Abstractions
{
    public interface IDataSeeder
    {
        Task SeedAsync(CancellationToken cancellationToken = default);
    }
}
