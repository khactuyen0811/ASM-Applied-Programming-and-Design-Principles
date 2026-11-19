using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIMS.Domain.Abstractions;
using SIMS.Infrastructure.Data;

namespace SIMS.Tests.Integration;

public sealed class SimsWebApplicationFactory : WebApplicationFactory<SIMS.Program>
{
    private readonly string _dataDirectory;
    private readonly string _connectionString;
    private readonly string? _previousDataDirectory;
    private readonly string? _previousConnectionString;

    public SimsWebApplicationFactory()
    {
        _dataDirectory = Path.Combine(Path.GetTempPath(), "sims-it-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dataDirectory);

        var databaseName = "SIMS_Test_" + Guid.NewGuid().ToString("N");
        _connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        _previousDataDirectory = Environment.GetEnvironmentVariable("Sims__DataDirectory");
        _previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SimsDb");
        Environment.SetEnvironmentVariable("Sims__DataDirectory", _dataDirectory);
        Environment.SetEnvironmentVariable("ConnectionStrings__SimsDb", _connectionString);

        using var scope = Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        seeder.SeedAsync().GetAwaiter().GetResult();
    }

    public string DataDirectory => _dataDirectory;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Sims:DataDirectory"] = _dataDirectory,
                ["ConnectionStrings:SimsDb"] = _connectionString
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                using var scope = Services.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<SimsDbContext>();
                database.Database.EnsureDeleted();
            }
            catch (Exception)
            {
            }

            Environment.SetEnvironmentVariable("Sims__DataDirectory", _previousDataDirectory);
            Environment.SetEnvironmentVariable("ConnectionStrings__SimsDb", _previousConnectionString);

            try
            {
                if (Directory.Exists(_dataDirectory))
                {
                    Directory.Delete(_dataDirectory, recursive: true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        base.Dispose(disposing);
    }
}

[CollectionDefinition("SimsIntegration")]
public sealed class SimsIntegrationCollection : ICollectionFixture<SimsWebApplicationFactory>
{
}
