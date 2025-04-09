using System.Text.Json.Serialization;

namespace WeatherMeasurementService.Models.ExternalApiModels
{
    /// <summary>
    /// Represents the overall structure of the external API response.
    /// </summary>
    public class WeatherApiResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("row_count")]
        public int RowCount { get; set; }

        [JsonPropertyName("result")]
        public List<WeatherApiResult> Result { get; set; } = [];
    }
}
