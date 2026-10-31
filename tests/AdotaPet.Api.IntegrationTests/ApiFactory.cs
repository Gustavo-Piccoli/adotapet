using AdotaPet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AdotaPet.Api.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Keep in sync with the sqlserver image in docker-compose.yml.
    private readonly MsSqlContainer _sql =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU27-ubuntu-22.04").Build();

    private readonly string _photosPath =
        Path.Combine(Path.GetTempPath(), "adotapet-tests-" + Guid.NewGuid().ToString("N"));

    public async Task InitializeAsync()
    {
        await _sql.StartAsync();

        // Accessing Services builds the host, which reads the settings from ConfigureWebHost.
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _sql.GetConnectionString());
        builder.UseSetting("Storage:PhotosPath", _photosPath);
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-with-more-than-32-chars");
        builder.UseSetting("Jwt:Issuer", "AdotaPet");
        builder.UseSetting("Jwt:Audience", "AdotaPet");
    }

    // xUnit v2's IAsyncLifetime.DisposeAsync returns Task, which clashes with the factory's ValueTask one.
    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _sql.DisposeAsync();
        if (Directory.Exists(_photosPath)) Directory.Delete(_photosPath, recursive: true);
    }
}
