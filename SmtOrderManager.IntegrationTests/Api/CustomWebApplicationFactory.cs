using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using SmtOrderManager.Infrastructure.Persistence;

namespace SmtOrderManager.IntegrationTests.Api;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Keep a single open SQLite in-memory connection for the host lifetime
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // Remove the AppDbContext registration from production
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            // Re-register DbContext to use the shared in-memory SQLite connection
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Ensure schema is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Optional: force environment for consistent ProblemDetails behavior in tests
        builder.UseEnvironment(Environments.Development);
    }
}
