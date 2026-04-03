using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sonderful.API.Data;

namespace Sonderful.Tests.Integration;

/// <summary>
/// Boots the real Sonderful.API in-process, swapping SQLite for an isolated
/// InMemory database. Each factory instance gets its own database so test
/// classes do not share state.
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "SonderfulTest_" + Guid.NewGuid();
    private readonly InMemoryDatabaseRoot _dbRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Provide a JWT key for tests
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "integration-test-secret-key-long-enough-for-hmac256"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing SQLite DbContext registration
            foreach (var d in services.Where(d =>
                d.ServiceType == typeof(AppDbContext) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GenericTypeArguments.Contains(typeof(AppDbContext)))).ToList())
            {
                services.Remove(d);
            }

            // Replace with InMemory database for testing
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName, _dbRoot)
                       .EnableServiceProviderCaching(false)
                       .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }
}
