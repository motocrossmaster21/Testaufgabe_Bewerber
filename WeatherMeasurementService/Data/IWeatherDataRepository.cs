using WeatherMeasurementService.Dtos;

namespace WeatherMeasurementService.Data
{
    public interface IWeatherDataRepository
    {
        /// <summary>
        /// Adds a new measurement (mapping from CreateMeasurementDto → WeatherData)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task AddMeasurementAsync(CreateMeasurementDto dto);

        /// <summary>
        /// Provides a list of measurements based on filters (e.g. station, period, sorting, limit, offset)
        /// </summary>
        /// <param name="station"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        Task<IEnumerable<WeatherDataDto>> GetMeasurementsAsync(
            string station, DateTime startDate, DateTime endDate, string sort, int limit, int offset);

        /// <summary>
        /// Provides aggregated statistics (min, max, avg, count)
        /// </summary>
        /// <param name="station"></param>
        /// <param name="measurementType"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<WeatherStatisticDto?> GetStatisticsAsync(
            string station, string measurementType, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Persists changes in the DB
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync();
    }
}
