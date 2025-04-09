using WeatherMeasurementService.Data;
using WeatherMeasurementService.Models.Dtos;

namespace WeatherMeasurementService.Services
{
    public class WeatherDataService(IWeatherDataRepository repository) : IWeatherDataService
    {
        private readonly IWeatherDataRepository _repository = repository;

        private static (DateTime Start, DateTime End) NormalizeTimeRange(WeatherDataQueryDto query)
        {
            var start = query.Start.Date;
            var end = query.End.Date.AddDays(1).AddTicks(-1);
            return (start, end);
        }

        private async Task ValidateQueryAsync(WeatherDataQueryDto query)
        {
            if (query.Start > query.End)
                throw new ArgumentException("Start date must be before or equal to end date.");

            if (query.End.Date > DateTime.UtcNow.Date)
                throw new ArgumentException("End date must not be in the future.");

            if (!await _repository.ExistsMeasurementTypeAsync(query.MeasurementType).ConfigureAwait(false))
                throw new MeasurementTypeNotFoundException(query.MeasurementType);

            if (!string.IsNullOrEmpty(query.Station) && !await _repository.ExistsStationAsync(query.Station).ConfigureAwait(false))
                throw new StationNotFoundException(query.Station);
        }

        public async Task<ResponseWeatherDataDto<WeatherDataDto?>> GetHighestAsync(WeatherDataQueryDto query)
        {
            await ValidateQueryAsync(query).ConfigureAwait(false);
            var (start, end) = NormalizeTimeRange(query);
            var dataset = await _repository.GetAllMeasurementsAsync(query.Station, start, end, query.MeasurementType).ConfigureAwait(false);
            var result = dataset.OrderByDescending(d => d.Value).FirstOrDefault();
            return new ResponseWeatherDataDto<WeatherDataDto?> { Result = result, Dataset = [.. dataset] };
        }

        public async Task<ResponseWeatherDataDto<WeatherDataDto?>> GetLowestAsync(WeatherDataQueryDto query)
        {
            await ValidateQueryAsync(query).ConfigureAwait(false);
            var (start, end) = NormalizeTimeRange(query);
            var dataset = await _repository.GetAllMeasurementsAsync(query.Station, start, end, query.MeasurementType).ConfigureAwait(false);
            var result = dataset.OrderBy(d => d.Value).FirstOrDefault();
            return new ResponseWeatherDataDto<WeatherDataDto?> { Result = result, Dataset = [.. dataset] };
        }

        public async Task<ResponseWeatherDataDto<double?>> GetAverageAsync(WeatherDataQueryDto query)
        {
            await ValidateQueryAsync(query).ConfigureAwait(false);
            var (start, end) = NormalizeTimeRange(query);
            var dataset = (await _repository.GetAllMeasurementsAsync(query.Station, start, end, query.MeasurementType).ConfigureAwait(false)).ToList();

            double? result = dataset.Count != 0 ? dataset.Average(d => (double?)d.Value) : null;

            return new ResponseWeatherDataDto<double?> { Result = result, Dataset = [.. dataset] };
        }

        public async Task<ResponseWeatherDataDto<int>> GetCountAsync(WeatherDataQueryDto query)
        {
            await ValidateQueryAsync(query).ConfigureAwait(false);
            var (start, end) = NormalizeTimeRange(query);
            var dataset = (await _repository.GetAllMeasurementsAsync(query.Station, start, end, query.MeasurementType).ConfigureAwait(false)).ToList();

            return new ResponseWeatherDataDto<int> { Result = dataset.Count, Dataset = [.. dataset] };
        }

        public async Task<ResponseWeatherDataDto<List<WeatherDataDto>>> GetAllAsync(WeatherDataQueryDto query)
        {
            await ValidateQueryAsync(query).ConfigureAwait(false);
            var (start, end) = NormalizeTimeRange(query);
            var dataset = await _repository.GetAllMeasurementsAsync(null, start, end, query.MeasurementType).ConfigureAwait(false);
            var filtered = string.IsNullOrEmpty(query.Station)
                ? dataset
                : dataset.Where(d => d.StationName == query.Station);
            return new ResponseWeatherDataDto<List<WeatherDataDto>> { Result = [.. filtered], Dataset = [.. dataset] };
        }

        public async Task<IEnumerable<string>> GetAllStationNamesAsync()
        {
            var stations = await _repository.GetAllStationsAsync().ConfigureAwait(false);
            return stations.Select(s => s.Name);
        }

        public async Task<IEnumerable<string>> GetAllMeasurementTypeNamesAsync()
        {
            var types = await _repository.GetAllMeasurementTypesAsync().ConfigureAwait(false);
            return types.Select(t => t.TypeName);
        }
    }

    public class MeasurementTypeNotFoundException(string type) : Exception($"Measurement type '{type}' does not exist.") { }
    public class StationNotFoundException(string station) : Exception($"Station '{station}' does not exist.") { }
}
