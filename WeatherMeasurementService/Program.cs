using WeatherMeasurementService.Data;
using Microsoft.EntityFrameworkCore;
using WeatherMeasurementService.Services;
using Serilog;
using Microsoft.OpenApi.Models;
using WeatherMeasurementService.Swagger;

namespace WeatherMeasurementService
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Use Serilog for logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
            builder.Host.UseSerilog();

            // Register controllers, OpenAPI, and other services.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherMeasurementService", Version = "v1" });
                c.EnableAnnotations();
                c.UseInlineDefinitionsForEnums();
                c.SchemaFilter<QuerrySchemaFilter>();// Combined schema filter for customizing Swagger query schemas
            });

            // Register DbContext with SQLite
            if (builder.Environment.EnvironmentName == "IntegrationTest")
            {
                builder.Services.AddDbContext<WeatherDbContext>(options =>
                    options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}"));
            }
            else
            {
                builder.Services.AddDbContext<WeatherDbContext>(options =>
                    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            }

            // Register WeatherData services.
            builder.Services.AddScoped<IWeatherDataService, WeatherDataService>();
            builder.Services.AddScoped<IWeatherDataRepository, WeatherDataRepository>();
            builder.Services.AddScoped<WeatherDataProcessingService>();

            // Register ExternalWeatherApiService as a typed HttpClient.
            builder.Services.AddHttpClient<ExternalWeatherApiClient>(client =>
            {
                var baseUrl = builder.Configuration["ExternalApi:BaseUrl"];
                client.BaseAddress = new Uri(baseUrl!);
            });

            // Register the hosted service to fetch weather data on startup.
            builder.Services.AddHostedService<WeatherDataFetcherHostedService>();

            var app = builder.Build();

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
            await context.Database.EnsureCreatedAsync().ConfigureAwait(false);  // Creates the database and schema if not exists

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherMeasurementService v1");
            });

            app.MapControllers();

            await app.RunAsync().ConfigureAwait(false);

        }

        // This protected constructor enables the WebApplicationFactory to create a copy of Program. (Test.InitDbHelper)
        protected Program() { }
    }
}