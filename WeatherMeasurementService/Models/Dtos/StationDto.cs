namespace WeatherMeasurementService.Models.Dtos
{
    /// <summary>
    /// This DTO is for providing station name to the client
    /// </summary>
    public class StationDto
    {
        public required string Name { get; set; }
    }
}
