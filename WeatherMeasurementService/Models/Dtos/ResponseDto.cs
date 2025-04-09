namespace WeatherMeasurementService.Models.Dtos
{
    /// <summary>
    /// This DTO is for providing weather data to the client (incl. dataset)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResponseWeatherDataDto<T>
    {
        public T? Result { get; set; }

        public required ICollection<WeatherDataDto> Dataset { get; set; } = [];
    }
}
