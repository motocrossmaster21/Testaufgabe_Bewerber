using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace WeatherMeasurementService.Models.Dtos
{
    /// <summary>
    /// Query parameters for retrieving weather data.
    /// </summary>
    public class WeatherDataQueryDto
    {
        [Required]
        [SwaggerSchema("Type of the measurement", Nullable = false)]
        public string MeasurementType { get; set; } = default!;

        [Required]
        [SwaggerSchema("Start date (format: yyyy-MM-dd). Time part will be set to 00:00:00.", Format = "date")]
        public DateTime Start { get; set; }

        [Required]
        [SwaggerSchema("End date (format: yyyy-MM-dd). Time part will be set to 23:59:59.", Format = "date")]
        public DateTime End { get; set; }

        [SwaggerSchema("Optional: Station name")]
        public string? Station { get; set; }
    }
}
