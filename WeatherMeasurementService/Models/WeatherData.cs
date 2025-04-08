namespace WeatherMeasurementService.Models
{
    /// <summary>
    /// WeatherData table: Actual measurements (potentially large table)
    /// </summary>
    public class WeatherData
    {
        public int WeatherDataId { get; set; }

        // Foreign keys
        public int StationId { get; set; }
        public required Station Station { get; set; }

        public int MeasurementTypeId { get; set; }
        public required MeasurementType MeasurementType { get; set; }

        // Actual measurement
        public required DateTime TimestampUtc { get; set; }    // e.g. 2025-04-05T21:50:00Z
        public required double Value { get; set; }             // e.g. 13.2, 9.8
        public required string Unit { get; set; }             // e.g. "°C", "hPa"
    }
}
