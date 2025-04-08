namespace WeatherMeasurementService.Dtos
{
    /// <summary>
    /// This DTO is for receiving new measurements from an external data source
    /// </summary>
    public class CreateMeasurementDto
    {
        // e.g. "Tiefenbrunnen"
        public required string StationName { get; set; }

        // e.g. "air_temperature"
        public required string TypeName { get; set; }

        // e.g. "°C"
        public required string Unit { get; set; }

        // e.g. 13.2
        public required double Value { get; set; }

        // e.g. "ok"
        public required string Status { get; set; }

        // e.g. "2025-04-05T21:50:00Z"
        public required DateTime TimestampUtc { get; set; }
    }
}
