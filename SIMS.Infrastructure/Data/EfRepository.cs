using Microsoft.EntityFrameworkCore;
using SIMS.Domain.Abstractions;

namespace SIMS.Infrastructure.Data
{
    public class EfRepository<T> : IRepository<T>, IRangeRepository<T>
        where T : class
    {
        protected readonly SimsDbContext Context;

        public EfRepository(SimsDbContext context)
        {
            Context = context;
        }

        protected DbSet<T> Set => Context.Set<T>();

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await Set.FindAsync(new object?[] { id }, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await Set.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await Set.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var idProperty = typeof(T).GetProperty("Id")
                ?? throw new InvalidOperationException($"Entity {typeof(T).Name} does not expose an Id property.");
            var id = (int)(idProperty.GetValue(entity)
                ?? throw new InvalidOperationException($"Entity {typeof(T).Name} has no Id value."));

            var existing = await Set.FindAsync(new object?[] { id }, cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                throw new KeyNotFoundException($"{typeof(T).Name} with id {id} was not found.");
            }

            Context.Entry(existing).CurrentValues.SetValues(entity);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await Set.FindAsync(new object?[] { id }, cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                return false;
            }

            Set.Remove(existing);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await Set.CountAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
