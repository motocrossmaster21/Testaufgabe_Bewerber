using System.Text.Json;
using WeatherMeasurementService.Models.ExternalApiModels;

namespace WeatherMeasurementService.Services
{
    public class ExternalWeatherApiClient(HttpClient httpClient, ILogger<ExternalWeatherApiClient> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<ExternalWeatherApiClient> _logger = logger;
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
            try
            {
                // Build the URL according to the documentation
                string queryString = $"{station}" +
                                    $"?startDate={startDate:yyyy-MM-dd}" +
                                    $"&endDate={endDate:yyyy-MM-dd}" +
                                    $"&sort={Uri.EscapeDataString(sort)}" + // Prevents problems with spaces or special characters (e.g. %20)
                                    $"&limit={limit}" +
                                    $"&offset={offset}";

                var url = $"measurements/{queryString}";

                HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);
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
