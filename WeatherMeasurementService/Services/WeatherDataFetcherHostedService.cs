﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Services
{
    /// <summary>
    /// This hosted service fetches weather data from the external API on application startup.
    /// </summary>
    public class WeatherDataFetcherHostedService(IServiceProvider services, ILogger<WeatherDataFetcherHostedService> logger) : IHostedService
    {
        private readonly IServiceProvider _services = services;
        private readonly ILogger<WeatherDataFetcherHostedService> _logger = logger;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting WeatherDataFetcherHostedService.");

            // Create a new scope to retrieve scoped services.
            using var scope = _services.CreateScope();
            var externalService = scope.ServiceProvider.GetRequiredService<ExternalWeatherApiService>();
            var processingService = scope.ServiceProvider.GetRequiredService<WeatherDataProcessingService>();

            // Determine the previous day's date range in UTC.
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var startDate = yesterday;
            var endDate = yesterday.AddDays(1);

            // Define the target stations.
            var stations = new[] { "tiefenbrunnen", "mythenquai" };
            foreach (var station in stations)
            {
                // Fetch data from external API with specified parameters:
                var apiResponse = await externalService.FetchWeatherDataAsync(
                    station,
                    startDate, // - Start and End Date (previous day)
                    endDate, // - Start and End Date (previous day)
                    "timestamp_cet desc",  // - Sort: "timestamp_cet desc" to get newest first
                    10, // - Limit: max 100 entries
                    0) // - Offset: 0 (start at the beginning)
                    .ConfigureAwait(false);

                if (apiResponse != null)
                {
                    // Process and store the fetched data.
                    await processingService.ProcessWeatherDataAsync(apiResponse).ConfigureAwait(false);
                    _logger.LogInformation("Processed data for station: {Station}", station);
                }
                else
                {
                    _logger.LogWarning("No data fetched for station: {Station}", station);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // No specific cleanup is required.
            _logger.LogInformation("Stopping WeatherDataFetcherHostedService.");
            return Task.CompletedTask;
        }
    }
}
