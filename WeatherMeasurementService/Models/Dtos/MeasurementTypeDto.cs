namespace WeatherMeasurementService.Models.Dtos
{
    /// <summary>
    /// This DTO is for providing measurementType to the client
    /// </summary>
    public class MeasurementTypeDto
    {
        public required string TypeName { get; set; }
        public required string DefaultUnit { get; set; }
    }
}
