using WeatherMeasurementService.Models.Dtos;

namespace WeatherMeasurementService.Services
{
    public interface IWeatherDataService
    {
        Task<ResponseWeatherDataDto<WeatherDataDto?>> GetHighestAsync(WeatherDataQueryDto query);
        Task<ResponseWeatherDataDto<WeatherDataDto?>> GetLowestAsync(WeatherDataQueryDto query);
        Task<ResponseWeatherDataDto<double?>> GetAverageAsync(WeatherDataQueryDto query);
        Task<ResponseWeatherDataDto<int>> GetCountAsync(WeatherDataQueryDto query);
        Task<ResponseWeatherDataDto<List<WeatherDataDto>>> GetAllAsync(WeatherDataQueryDto query);
        Task<IEnumerable<string>> GetAllStationNamesAsync();
        Task<IEnumerable<string>> GetAllMeasurementTypeNamesAsync();

    }
}
