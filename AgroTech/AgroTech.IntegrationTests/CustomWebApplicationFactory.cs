using AgroTech.Domain.Entities;
using AgroTech.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AgroTech.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private static readonly InMemoryDatabaseRoot _databaseRoot = new();
        private static bool _databaseInitialized;
        private static readonly object _lock = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RabbitMq:Enabled"] = "false"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AgroTechDbContext>));
                services.RemoveAll(typeof(AgroTechDbContext));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<AgroTechDbContext>));

                services.AddDbContext<AgroTechDbContext>(options =>
                {
                    options.UseInMemoryDatabase("AgroTechIntegrationTestsDb", _databaseRoot);
                });

                var serviceProvider = services.BuildServiceProvider();

                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AgroTechDbContext>();

                lock (_lock)
                {
                    if (!_databaseInitialized)
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                        SeedDatabase(context);
                        _databaseInitialized = true;
                    }
                }
            });
        }

        private static void SeedDatabase(AgroTechDbContext context)
        {
            if (context.Sensors.Any())
                return;

            context.Sensors.AddRange(
                new Sensor
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Temperatura",
                    Type = 1,
                    Value = 25.4,
                    Timestamp = new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow
                },
                new Sensor
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Umidade",
                    Type = 2,
                    Value = 60,
                    Timestamp = new DateTime(2026, 4, 7, 10, 5, 0, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow
                },
                new Sensor
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Ph Solo",
                    Type = 3,
                    Value = 6.5,
                    Timestamp = new DateTime(2026, 4, 7, 10, 10, 0, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.SaveChanges();
        }
    }
}