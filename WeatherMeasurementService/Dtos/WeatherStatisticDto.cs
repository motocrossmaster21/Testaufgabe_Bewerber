namespace WeatherMeasurementService.Dtos
{
    /// <summary>
    /// For aggregated stats like min, max, avg, count
    /// </summary>
    public class WeatherStatisticDto
    {
        // optional
        public required string StationName { get; set; }

        // e.g. "air_temperature"
        public required string MeasurementType { get; set; }
        public required double MinValue { get; set; }
        public required double MaxValue { get; set; }
        public required double AvgValue { get; set; }
        public required int Count { get; set; }

        // Optional: time range
        public required DateTime RangeStart { get; set; }
        public required DateTime RangeEnd { get; set; }
    }
}
