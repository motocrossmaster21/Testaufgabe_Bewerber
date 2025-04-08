using WeatherMeasurementService.Data;
using Microsoft.EntityFrameworkCore;
using WeatherMeasurementService.Services;
using Serilog;
using Serilog.Events;



var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Register controllers, OpenAPI, and other services.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register DbContext with SQLite
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register WeatherData services.
builder.Services.AddScoped<IWeatherDataRepository, WeatherDataRepository>();
builder.Services.AddScoped<WeatherDataProcessingService>();

// Register ExternalWeatherApiService as a typed HttpClient.
builder.Services.AddHttpClient<ExternalWeatherApiService>();

// Register the hosted service to fetch weather data on startup.
builder.Services.AddHostedService<WeatherDataFetcherHostedService>();


var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
context.Database.EnsureCreated();  // Creates the database and schema if not exists

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();