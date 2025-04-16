using WeatherMeasurementService.Models.Dtos;

namespace WeatherMeasurementService.Data
{
    public interface IWeatherDataRepository
    {
        /// <summary>
        /// Adds a new measurement (mapping from CreateMeasurementDto → WeatherData)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task AddMeasurementAsync(ImportedMeasurementDto dto);

        /// <summary>
        /// Persists changes in the DB
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Returns all measurements for the given parameters
        /// </summary>
        /// <param name="station"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="measurementType"></param>
        /// <returns></returns>
        Task<IEnumerable<WeatherDataDto>> GetAllMeasurementsAsync(string? station, DateTime start, DateTime end, string measurementType);

        /// <summary>
        /// Returns all available measurement types
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MeasurementTypeDto>> GetAllMeasurementTypesAsync();

        /// <summary>
        /// Returns all available stations
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<StationDto>> GetAllStationsAsync();

        /// <summary>
        /// Checks if a specific MeasurementType exists
        /// </summary>
        /// <param name="measurementType"></param>
        /// <returns></returns>
        Task<bool> ExistsMeasurementTypeAsync(string measurementType);

        /// <summary>
        /// Checks if a specific Station exists
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        Task<bool> ExistsStationAsync(string stationName);
    }
}
