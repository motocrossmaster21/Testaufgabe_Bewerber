using Microsoft.EntityFrameworkCore;
using WeatherMeasurementService.Models.Daos;
using WeatherMeasurementService.Models.Dtos;

namespace WeatherMeasurementService.Data
{
    public class WeatherDataRepository(WeatherDbContext context, ILogger<WeatherDataRepository> logger) : IWeatherDataRepository
    {
        private readonly ILogger<WeatherDataRepository> _logger = logger;
        private readonly WeatherDbContext _context = context;

        // Simple in-memory caches for the duration of the request
        private readonly Dictionary<string, StationDao> _stationCache = [];
        private readonly Dictionary<string, MeasurementTypeDao> _measurementTypeCache = [];

        public async Task AddMeasurementAsync(ImportedMeasurementDto dto)
        {
            // Filter out invalid measurements.
            if (!dto.Status.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogError("Invalid measurement received: Station {Station}, Type {Type}, Value {Value}, Status {Status}",
                    dto.StationName, dto.TypeName, dto.Value, dto.Status);
                return;
            }

            // Check the cache for the station.
            if (!_stationCache.TryGetValue(dto.StationName, out StationDao? station))
            {
                // Search locally (in the change tracker) first, then in the database.
                station = _context.Stations.Local.FirstOrDefault(s => s.Name == dto.StationName)
                          ?? await _context.Stations.FirstOrDefaultAsync(s => s.Name == dto.StationName).ConfigureAwait(false)
                          ?? new StationDao { Name = dto.StationName };
                _stationCache[dto.StationName] = station;
            }

            // Check the cache for measurement type.
            if (!_measurementTypeCache.TryGetValue(dto.TypeName, out MeasurementTypeDao? measurementType))
            {
                measurementType = _context.MeasurementTypes.Local.FirstOrDefault(mt => mt.TypeName == dto.TypeName)
                                   ?? await _context.MeasurementTypes.FirstOrDefaultAsync(mt => mt.TypeName == dto.TypeName).ConfigureAwait(false)
                                   ?? new MeasurementTypeDao { TypeName = dto.TypeName, DefaultUnit = dto.Unit };
                _measurementTypeCache[dto.TypeName] = measurementType;
            }

            // Check for an existing measurement to avoid duplicates.
            var existingMeasurement = await _context.WeatherData
                .FirstOrDefaultAsync(m => m.Station.Name == dto.StationName &&
                                           m.MeasurementType.TypeName == dto.TypeName &&
                                           m.TimestampUtc == dto.TimestampUtc)
                .ConfigureAwait(false);
            if (existingMeasurement != null)
            {
                _logger.LogDebug("Duplicate measurement detected for station {Station}, type {Type} at {TimestampUtc}. Skipping insertion.",
                    dto.StationName, dto.TypeName, dto.TimestampUtc);
                return;
            }

            var measurement = new WeatherDataDao
            {
                Station = station,
                MeasurementType = measurementType,
                TimestampUtc = dto.TimestampUtc,
                Value = dto.Value,
                Unit = dto.Unit
            };

            _context.WeatherData.Add(measurement);
        }

        public async Task<IEnumerable<WeatherDataDto>> GetAllMeasurementsAsync(string? station, DateTime start, DateTime end, string measurementType)
        {
            var query = _context.WeatherData
                .Include(m => m.Station)
                .Include(m => m.MeasurementType)
                .Where(m => m.MeasurementType.TypeName == measurementType &&
                            m.TimestampUtc >= start && m.TimestampUtc <= end);

            if (!string.IsNullOrWhiteSpace(station))
            {
                query = query.Where(m => m.Station.Name == station);
            }

            var measurements = await query.ToListAsync().ConfigureAwait(false);

            return measurements.Select(m => new WeatherDataDto
            {
                StationName = m.Station.Name,
                MeasurementType = m.MeasurementType.TypeName,
                Value = m.Value,
                Unit = m.Unit,
                TimestampUtc = m.TimestampUtc
            });
        }

        public async Task<IEnumerable<MeasurementTypeDto>> GetAllMeasurementTypesAsync()
        {
            var types = await _context.MeasurementTypes.ToListAsync();
            return types.Select(t => new MeasurementTypeDto
            {
                TypeName = t.TypeName,
                DefaultUnit = t.DefaultUnit
            });
        }

        public async Task<IEnumerable<StationDto>> GetAllStationsAsync()
        {
            var stations = await _context.Stations.ToListAsync();
            return stations.Select(s => new StationDto
            {
                Name = s.Name
            });
        }

        public async Task<bool> ExistsMeasurementTypeAsync(string measurementType)
        {
            return await _context.MeasurementTypes
                .AnyAsync(mt => mt.TypeName == measurementType);
        }

        public async Task<bool> ExistsStationAsync(string stationName)
        {
            return await _context.Stations
                .AnyAsync(s => s.Name == stationName);
        }

        public async Task<int> SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes to the database.");
            return await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
