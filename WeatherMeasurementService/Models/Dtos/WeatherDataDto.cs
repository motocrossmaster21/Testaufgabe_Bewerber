namespace WeatherMeasurementService.Models.Dtos
{
    /// <summary>
    /// This DTO is for providing measurement data to the client
    /// </summary>
    public class WeatherDataDto
    {
        public required string StationName { get; set; }
        public required string MeasurementType { get; set; }
        public required double Value { get; set; }
        public required string Unit { get; set; }
        public required DateTime TimestampUtc { get; set; }
    }
}
