namespace WeatherMeasurementService.Models.Daos
{
    /// <summary>
    /// MeasurementTypeDao table: Each type of measurement (air temp, water temp, etc.)
    /// </summary>
    public class MeasurementTypeDao
    {
        public int MeasurementTypeId { get; set; }

        // e.g. "air_temperature"
        public required string TypeName { get; set; }

        // e.g. "°C"
        public required string DefaultUnit { get; set; }

        // Navigation property: 1 measurement type can be used in many WeatherData records
        public ICollection<WeatherDataDao>? Measurements { get; set; }
    }
}
