using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _testDbPath;

    public CustomWebApplicationFactory()
    {
        _testDbPath = $"test_todo_{Guid.NewGuid()}.db";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Suppress info and below logs during tests (only show warnings and errors)
        builder.ConfigureLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TodoDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add SQLite test database with unique name per test
            var connectionString = $"Data Source={_testDbPath}";
            services.AddDbContext<TodoDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });
        });
    }

    public void EnsureDatabaseCreated()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        db.Database.EnsureCreated();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up the test database file
            try
            {
                if (File.Exists(_testDbPath))
                {
                    File.Delete(_testDbPath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        base.Dispose(disposing);
    }
}
