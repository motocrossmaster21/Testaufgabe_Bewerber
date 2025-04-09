using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WeatherMeasurementService.Data;

namespace WeatherMeasurementService.Tests
{
    public class InitDbHelper : IAsyncLifetime
    {
        public WebApplicationFactory<Program> Factory { get; }
        public HttpClient Client { get; }

        public InitDbHelper()
        {
            Factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("IntegrationTest");
                    builder.ConfigureServices(services =>
                    {
                        // Remove all existing registrations of the DbContext
                        services.RemoveAll<WeatherDbContext>();
                        services.RemoveAll<DbContextOptions<WeatherDbContext>>();
                        // Register the DbContext with an InMemory database
                        services.AddDbContext<WeatherDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("IntegrationTestDb");
                        });
                    });
                });



            Client = Factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            await ClearDatabaseAsync();
        }

        public async Task DisposeAsync()
        {
            await ClearDatabaseAsync();
            await Factory.DisposeAsync();
        }

        public async Task ClearDatabaseAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }
    }

    [CollectionDefinition("Integration Tests")]
    public class IntegrationTestsCollection : ICollectionFixture<InitDbHelper>
    {
        // This class does not require any code. It is only used to assign the fixture data to the collection.
    }
}
