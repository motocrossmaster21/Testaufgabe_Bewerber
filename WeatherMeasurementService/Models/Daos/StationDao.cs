namespace WeatherMeasurementService.Models.Daos
{
    /// <summary>
    /// Station table: Master data for each weather station
    /// </summary>
    public class StationDao
    {
        public int StationId { get; set; }

        // e.g. "Tiefenbrunnen" / "Mythenquai"
        public required string Name { get; set; }

        // Navigation property: 1 station can have many WeatherData entries
        public ICollection<WeatherDataDao>? Measurements { get; set; }
    }
}
