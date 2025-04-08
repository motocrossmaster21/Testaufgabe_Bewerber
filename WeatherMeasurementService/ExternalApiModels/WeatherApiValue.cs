using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherMeasurementService.ExternalApiModels
{
    /// <summary>
    /// Represents an individual measurement value (e.g. "air_temperature", "water_temperature").
    /// </summary>
    public class WeatherApiValue
    {
        // Use JsonElement to handle values that could be a number, string or null. (e.g. timestamp_cet, precipitation)
        [JsonPropertyName("value")]
        public JsonElement Value { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
