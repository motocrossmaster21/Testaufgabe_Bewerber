using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeatherMeasurementService.Dtos;
using WeatherMeasurementService.Models;

namespace WeatherMeasurementService.Data
{
    public class WeatherDataRepository(WeatherDbContext context, ILogger<WeatherDataRepository> logger) : IWeatherDataRepository
    {
        private readonly ILogger<WeatherDataRepository> _logger = logger;
        private readonly WeatherDbContext _context = context;

        // Simple in-memory caches for the duration of the request
        private readonly Dictionary<string, Station> _stationCache = [];
        private readonly Dictionary<string, MeasurementType> _measurementTypeCache = [];

        public async Task AddMeasurementAsync(CreateMeasurementDto dto)
        {
            // Filter out invalid measurements.
            if (!dto.Status.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogError("Invalid measurement received: Station {Station}, Type {Type}, Value {Value}, Status {Status}",
                    dto.StationName, dto.TypeName, dto.Value, dto.Status);
                return;
            }

            // Check the cache for the station.
            if (!_stationCache.TryGetValue(dto.StationName, out Station? station))
            {
                // Search locally (in the change tracker) first, then in the database.
                station = _context.Stations.Local.FirstOrDefault(s => s.Name == dto.StationName)
                          ?? await _context.Stations.FirstOrDefaultAsync(s => s.Name == dto.StationName).ConfigureAwait(false)
                          ?? new Station { Name = dto.StationName };
                _stationCache[dto.StationName] = station;
            }

            // Check the cache for measurement type.
            if (!_measurementTypeCache.TryGetValue(dto.TypeName, out MeasurementType? measurementType))
            {
                measurementType = _context.MeasurementTypes.Local.FirstOrDefault(mt => mt.TypeName == dto.TypeName)
                                   ?? await _context.MeasurementTypes.FirstOrDefaultAsync(mt => mt.TypeName == dto.TypeName).ConfigureAwait(false)
                                   ?? new MeasurementType { TypeName = dto.TypeName, DefaultUnit = dto.Unit };
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

            // Create the new WeatherData entry
            var measurement = new WeatherData
            {
                Station = station,
                MeasurementType = measurementType,
                TimestampUtc = dto.TimestampUtc,
                Value = (double)dto.Value,
                Unit = dto.Unit
            };

            _context.WeatherData.Add(measurement);
        }

        public async Task<IEnumerable<WeatherDataDto>> GetMeasurementsAsync(string station, DateTime startDate, DateTime endDate, string sort, int limit, int offset)
        {
            // Query incl. navigation properties
            var query = _context.WeatherData
                .Include(m => m.Station)
                .Include(m => m.MeasurementType)
                .Where(m => m.Station.Name == station && m.TimestampUtc >= startDate && m.TimestampUtc <= endDate);

            // Sort by timestamp
            if (sort.ToLower().Contains("desc",StringComparison.OrdinalIgnoreCase))
            {
                query = query.OrderByDescending(m => m.TimestampUtc);
            }
            else
            {
                query = query.OrderBy(m => m.TimestampUtc);
            }

            var measurements = await query.Skip(offset).Take(limit).ToListAsync().ConfigureAwait(false);

            // Mapping: Entity → DTO
            return measurements.Select(m => new WeatherDataDto
            {
                StationName = m.Station.Name,
                MeasurementType = m.MeasurementType.TypeName,
                Value = m.Value,
                Unit = m.Unit,
                TimestampUtc = m.TimestampUtc
            });
        }

        public async Task<WeatherStatisticDto?> GetStatisticsAsync(string station, string measurementType, DateTime startDate, DateTime endDate)
        {
            var query = _context.WeatherData
                .Include(m => m.Station)
                .Include(m => m.MeasurementType)
                .Where(m => m.Station.Name == station &&
                            m.MeasurementType.TypeName == measurementType &&
                            m.TimestampUtc >= startDate &&
                            m.TimestampUtc <= endDate);

            var count = await query.CountAsync().ConfigureAwait(false);
            if (count == 0)
            {
                // No data found, returning null.
                return null;
            }

            var min = await query.MinAsync(m => m.Value).ConfigureAwait(false);
            var max = await query.MaxAsync(m => m.Value).ConfigureAwait(false);
            var avg = await query.AverageAsync(m => m.Value).ConfigureAwait(false);

            return new WeatherStatisticDto
            {
                StationName = station,
                MeasurementType = measurementType,
                Count = count,
                MinValue = min,
                MaxValue = max,
                AvgValue = avg,
                RangeStart = startDate,
                RangeEnd = endDate
            };
        }

        public async Task<int> SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes to the database.");
            return await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
