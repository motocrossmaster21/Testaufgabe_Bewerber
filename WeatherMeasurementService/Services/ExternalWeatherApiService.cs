using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeatherMeasurementService.Data;
using WeatherMeasurementService.Dtos;
using WeatherMeasurementService.ExternalApiModels;

namespace WeatherMeasurementService.Services
{
    public class ExternalWeatherApiService(HttpClient httpClient, ILogger<ExternalWeatherApiService> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<ExternalWeatherApiService> _logger = logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// This method fetches data from: 
        /// GET /measurements/{station}?startDate=...&endDate=...&sort=...&limit=...&offset=...
        /// </summary>
        /// <param name="station"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async Task<WeatherApiResponse?> FetchWeatherDataAsync(
            string station,
            DateTime startDate,
            DateTime endDate,
            string sort,
            int limit,
            int offset)
        {
            // Example: 
            // https://tecdottir.metaodi.ch/measurements/tiefenbrunnen
            //   ?startDate=2025-03-07&endDate=2025-03-08
            //   &sort=timestamp_cet%20desc
            //   &limit=100
            //   &offset=2

            // Build the URL according to the documentation
            string baseUrl = "https://tecdottir.metaodi.ch/measurements";
            string requestUrl = $"{baseUrl}/{station}" +
                                $"?startDate={startDate:yyyy-MM-dd}" +
                                $"&endDate={endDate:yyyy-MM-dd}" +
                                $"&sort={Uri.EscapeDataString(sort)}" + // Prevents problems with spaces or special characters (e.g. %20)
                                $"&limit={limit}" +
                                $"&offset={offset}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch data. Status code: {Code}", response.StatusCode);
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                // Deserialize into strongly typed WeatherApiResponse
                var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(jsonString, _jsonSerializerOptions);

                if (apiResponse == null)
                {
                    _logger.LogError("WeatherApiResponse is null after deserialization");
                    return null;
                }

                // Check if the response is "ok": true
                if (!apiResponse.Ok)
                {
                    _logger.LogWarning("API returned 'ok': false. Message: {Message}", apiResponse.Message);
                    return null;
                }

                return apiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during FetchWeatherDataAsync");
                return null;
            }
        }
    }
}
