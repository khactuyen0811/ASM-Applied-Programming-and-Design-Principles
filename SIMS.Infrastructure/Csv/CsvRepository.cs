using System.Globalization;
using System.Reflection;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using SIMS.Domain.Abstractions;
using SIMS.Infrastructure.Options;

namespace SIMS.Infrastructure.Csv
{
    public class CsvRepository<T> : IRepository<T>, IRangeRepository<T>
        where T : class, new()
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly string _filePath;
        private readonly PropertyInfo? _idProperty;
        private readonly PropertyInfo? _createdAtProperty;
        private List<T>? _cache;

        protected CsvRepository(CsvOptions options, string fileName)
        {
            ArgumentNullException.ThrowIfNull(options);
            if (string.IsNullOrWhiteSpace(options.DataDirectory))
            {
                throw new ArgumentException("Data directory must be configured.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name is required.", nameof(fileName));
            }

            Directory.CreateDirectory(options.DataDirectory);
            _filePath = Path.Combine(options.DataDirectory, fileName);

            _idProperty = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            _createdAtProperty = typeof(T).GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance);

            EnsureFileExists();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            foreach (var item in snapshot)
            {
                if (GetId(item) == id)
                {
                    return item;
                }
            }

            return null;
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await MutateAsync(list =>
            {
                if (GetId(entity) <= 0)
                {
                    SetId(entity, NextId(list));
                }

                ApplyCreatedAt(entity);
                list.Add(entity);
                return true;
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var incoming = entities as IReadOnlyCollection<T> ?? entities.ToList();
            if (incoming.Count == 0)
            {
                return;
            }

            await MutateAsync(list =>
            {
                var next = NextId(list);
                foreach (var entity in incoming)
                {
                    if (GetId(entity) <= 0)
                    {
                        SetId(entity, next++);
                    }
                    else if (GetId(entity) >= next)
                    {
                        next = GetId(entity) + 1;
                    }

                    ApplyCreatedAt(entity);
                    list.Add(entity);
                }

                return true;
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var id = GetId(entity);
            var found = false;

            await MutateAsync(list =>
            {
                var index = list.FindIndex(item => GetId(item) == id);
                if (index < 0)
                {
                    return false;
                }

                list[index] = entity;
                found = true;
                return true;
            }, cancellationToken).ConfigureAwait(false);

            if (!found)
            {
                throw new KeyNotFoundException($"Entity with Id '{id}' was not found.");
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var removed = false;

            await MutateAsync(list =>
            {
                var index = list.FindIndex(item => GetId(item) == id);
                if (index < 0)
                {
                    return false;
                }

                list.RemoveAt(index);
                removed = true;
                return true;
            }, cancellationToken).ConfigureAwait(false);

            return removed;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            return snapshot.Count;
        }

        protected async Task<IReadOnlyList<T>> GetSnapshotAsync(CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var list = await LoadUnlockedAsync(cancellationToken).ConfigureAwait(false);
                return list.ToArray();
            }
            finally
            {
                _gate.Release();
            }
        }

        protected async Task MutateAsync(Func<List<T>, bool> mutation, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(mutation);

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var list = await LoadUnlockedAsync(cancellationToken).ConfigureAwait(false);
                if (!mutation(list))
                {
                    return;
                }

                await PersistUnlockedAsync(list, cancellationToken).ConfigureAwait(false);
                _cache = list;
            }
            finally
            {
                _gate.Release();
            }
        }

        protected async Task SaveAsync(IReadOnlyList<T> items, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(items);

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await PersistUnlockedAsync(items, cancellationToken).ConfigureAwait(false);
                _cache = items as List<T> ?? new List<T>(items);
            }
            finally
            {
                _gate.Release();
            }
        }

        protected virtual int GetId(T entity)
        {
            if (_idProperty is null)
            {
                return 0;
            }

            var value = _idProperty.GetValue(entity);
            return value is int id ? id : 0;
        }

        private void SetId(T entity, int id)
        {
            _idProperty?.SetValue(entity, id);
        }

        private void ApplyCreatedAt(T entity)
        {
            if (_createdAtProperty is null || _createdAtProperty.PropertyType != typeof(DateTime))
            {
                return;
            }

            var value = (DateTime?)_createdAtProperty.GetValue(entity);
            if (value is null || value.Value == default)
            {
                _createdAtProperty.SetValue(entity, DateTime.UtcNow);
            }
        }

        private int NextId(IReadOnlyList<T> list)
        {
            var max = 0;
            foreach (var item in list)
            {
                var id = GetId(item);
                if (id > max)
                {
                    max = id;
                }
            }

            return max + 1;
        }

        private async Task<List<T>> LoadUnlockedAsync(CancellationToken cancellationToken)
        {
            if (_cache is not null)
            {
                return _cache;
            }

            var list = new List<T>();
            if (File.Exists(_filePath))
            {
                await using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                using var reader = new StreamReader(stream, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);
                using var csv = new CsvReader(reader, CreateConfiguration());
                ApplyTypeConverters(csv.Context);
                await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken).ConfigureAwait(false))
                {
                    list.Add(record);
                }
            }

            _cache = list;
            return list;
        }

        private async Task PersistUnlockedAsync(IReadOnlyList<T> items, CancellationToken cancellationToken)
        {
            var tempPath = _filePath + ".tmp";

            await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { NewLine = "\r\n" })
            using (var csv = new CsvWriter(writer, CreateConfiguration()))
            {
                ApplyTypeConverters(csv.Context);
                csv.WriteHeader<T>();
                await csv.NextRecordAsync().ConfigureAwait(false);
                await csv.WriteRecordsAsync(items, cancellationToken).ConfigureAwait(false);
            }

            File.Move(tempPath, _filePath, overwrite: true);
        }

        private void EnsureFileExists()
        {
            if (File.Exists(_filePath))
            {
                return;
            }

            using var stream = new FileStream(_filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { NewLine = "\r\n" };
            using var csv = new CsvWriter(writer, CreateConfiguration());
            ApplyTypeConverters(csv.Context);
            csv.WriteHeader<T>();
            csv.NextRecord();
        }

        private static CsvConfiguration CreateConfiguration()
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLowerInvariant(),
                HasHeaderRecord = true,
                NewLine = "\r\n"
            };

            return configuration;
        }

        private static void ApplyTypeConverters(CsvContext context)
        {
            context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = new[] { "O" };
            context.TypeConverterOptionsCache.GetOptions<DateTime?>().Formats = new[] { "O" };
        }
    }
}
