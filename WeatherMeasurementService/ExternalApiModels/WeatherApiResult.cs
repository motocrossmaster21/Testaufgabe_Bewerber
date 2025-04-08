using System.Text.Json.Serialization;

namespace WeatherMeasurementService.ExternalApiModels
{
    /// <summary>
    /// Represents a single record within the "result" array of the external API.
    /// </summary>
    public class WeatherApiResult
    {
        [JsonPropertyName("station")]
        public string? Station { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("values")]
        public Dictionary<string, WeatherApiValue> Values { get; set; } = [];
    }
}
