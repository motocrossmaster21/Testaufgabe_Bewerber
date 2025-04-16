using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherMeasurementService.Data;
using WeatherMeasurementService.Models.Dtos;

namespace WeatherMeasurementService.Tests
{
    public class WeatherDataRepositoryTests : IDisposable
    {
        private readonly WeatherDbContext _context;
        private readonly WeatherDataRepository _repository;

        public WeatherDataRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WeatherDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            _context = new WeatherDbContext(options);
            var logger = new Mock<ILogger<WeatherDataRepository>>().Object;
            _repository = new WeatherDataRepository(_context, logger);
        }

        [Fact]
        public async Task AddMeasurementAsync_WithoutStatusOk_SavesMeasurement()
        {
            // Arrange
            var dto = new ImportedMeasurementDto
            {
                StationName = "TestStation",
                TypeName = "humidity",
                Unit = "%",
                Value = 55.0,
                Status = "error", // Invalid measurement
                TimestampUtc = DateTime.UtcNow
            };

            // Act
            await _repository.AddMeasurementAsync(dto);
            await _repository.SaveChangesAsync();

            Assert.Empty(_context.WeatherData);
            Assert.Empty(_context.Stations);
            Assert.Empty(_context.MeasurementTypes);
        }

        [Fact]
        public async Task AddMeasurementAsync_CreatesStationAndTypeIfNotInDbOrCache()
        {
            var dto = new ImportedMeasurementDto
            {
                StationName = "NeuStation",
                TypeName = "wind_speed",
                Unit = "m/s",
                Value = 5.5,
                Status = "ok",
                TimestampUtc = DateTime.UtcNow
            };

            await _repository.AddMeasurementAsync(dto);
            await _repository.SaveChangesAsync();

            var entry = await _context.WeatherData
                .Include(w => w.Station)
                .Include(w => w.MeasurementType)
                .FirstAsync();
            Assert.Single(_context.WeatherData);
            Assert.Single(_context.Stations);
            Assert.Single(_context.MeasurementTypes);
            Assert.Equal("NeuStation", entry.Station.Name);
            Assert.Equal("wind_speed", entry.MeasurementType.TypeName);
        }

        [Fact]
        public async Task AddMeasurementAsync_UsesCachedStationAndTypeOnSecondInsert()
        {
            var dto1 = new ImportedMeasurementDto
            {
                StationName = "CachedStation",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 18.0,
                Status = "ok",
                TimestampUtc = DateTime.UtcNow
            };

            var dto2 = new ImportedMeasurementDto
            {
                StationName = dto1.StationName,
                TypeName = dto1.TypeName,
                Unit = dto1.Unit,
                Value = 19.0,
                Status = dto1.Status,
                TimestampUtc = dto1.TimestampUtc.AddMinutes(5)
            };

            await _repository.AddMeasurementAsync(dto1);
            await _repository.AddMeasurementAsync(dto2);
            await _repository.SaveChangesAsync();

            var entries = await _context.WeatherData
                .Include(w => w.Station)
                .Include(w => w.MeasurementType)
                .ToListAsync();

            Assert.Equal(2, entries.Count);
            Assert.Single(_context.Stations);
            Assert.Single(_context.MeasurementTypes);
        }

        [Fact]
        public async Task AddMeasurementAsync_SavesValidMeasurement()
        {
            // Arrange
            var dto = new ImportedMeasurementDto
            {
                StationName = "TestStation",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 20.0,
                Status = "ok",
                TimestampUtc = DateTime.UtcNow
            };

            // Act
            await _repository.AddMeasurementAsync(dto);
            await _repository.SaveChangesAsync();

            // Assert
            Assert.Single(_context.WeatherData);
            var entry = await _context.WeatherData.Include(w => w.Station).Include(w => w.MeasurementType).FirstAsync();
            Assert.Equal(20.0, entry.Value);
            Assert.Equal("TestStation", entry.Station.Name);
            Assert.Equal("air_temperature", entry.MeasurementType.TypeName);
        }

        [Fact]
        public async Task GetAllMeasurementsAsync_ReturnsMeasurementsInRange()
        {
            var now = DateTime.UtcNow;

            await _repository.AddMeasurementAsync(new ImportedMeasurementDto
            {
                StationName = "S1",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 15,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            });

            await _repository.SaveChangesAsync();

            var result = await _repository.GetAllMeasurementsAsync("S1", now.AddHours(-2), now, "air_temperature");

            Assert.Single(result);
            var measurement = result.First();
            Assert.Equal("S1", measurement.StationName);
            Assert.Equal("air_temperature", measurement.MeasurementType);
        }

        [Fact]
        public async Task GetAllMeasurementsAsync_WithWrongStation_ReturnsEmpty()
        {
            var now = DateTime.UtcNow;

            await _repository.AddMeasurementAsync(new ImportedMeasurementDto
            {
                StationName = "S1",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 20,
                Status = "ok",
                TimestampUtc = now
            });

            await _repository.SaveChangesAsync();

            var result = await _repository.GetAllMeasurementsAsync("UnknownStation", now.AddHours(-1), now.AddHours(1), "air_temperature");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllMeasurementsAsync_OutsideDateRange_ReturnsEmpty()
        {
            var now = DateTime.UtcNow;

            await _repository.AddMeasurementAsync(new ImportedMeasurementDto
            {
                StationName = "S1",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 22,
                Status = "ok",
                TimestampUtc = now.AddDays(-3)
            });

            await _repository.SaveChangesAsync();

            var result = await _repository.GetAllMeasurementsAsync("S1", now.AddHours(-1), now.AddHours(1), "air_temperature");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllMeasurementsAsync_WrongType_ReturnsEmpty()
        {
            var now = DateTime.UtcNow;

            await _repository.AddMeasurementAsync(new ImportedMeasurementDto
            {
                StationName = "S1",
                TypeName = "humidity",
                Unit = "%",
                Value = 45,
                Status = "ok",
                TimestampUtc = now
            });

            await _repository.SaveChangesAsync();

            var result = await _repository.GetAllMeasurementsAsync("S1", now.AddHours(-1), now.AddHours(1), "air_temperature");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllMeasurementsAsync_WithoutStationFilter_ReturnsMatchingTypeFromAllStations()
        {
            var now = DateTime.UtcNow;

            await _repository.AddMeasurementAsync(new ImportedMeasurementDto
            {
                StationName = "S1",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 15,
                Status = "ok",
                TimestampUtc = now
            });

            await _repository.AddMeasurementAsync(new ImportedMeasurementDto
            {
                StationName = "S2",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 17,
                Status = "ok",
                TimestampUtc = now
            });

            await _repository.SaveChangesAsync();

            var result = await _repository.GetAllMeasurementsAsync(null, now.AddHours(-1), now.AddHours(1), "air_temperature");

            Assert.Equal(2, result.Count());
        }


        [Fact]
        public async Task ExistsMeasurementTypeAsync_ReturnsTrueForExistingType()
        {
            var dto = new ImportedMeasurementDto
            {
                StationName = "TestStation",
                TypeName = "humidity",
                Unit = "%",
                Value = 55,
                Status = "ok",
                TimestampUtc = DateTime.UtcNow
            };

            await _repository.AddMeasurementAsync(dto);
            await _repository.SaveChangesAsync();

            var exists = await _repository.ExistsMeasurementTypeAsync("humidity");

            Assert.True(exists);
        }

        [Fact]
        public async Task GetAllMeasurementTypesAsync_ReturnsAllTypes()
        {
            // Arrange
            _context.MeasurementTypes.Add(new()
            {
                TypeName = "air_temperature",
                DefaultUnit = "°C"
            });
            _context.MeasurementTypes.Add(new()
            {
                TypeName = "humidity",
                DefaultUnit = "%"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllMeasurementTypesAsync();

            // Assert
            Assert.Equal(2, result.Count());

            var types = result.ToList();
            Assert.Contains(types, t => t.TypeName == "air_temperature" && t.DefaultUnit == "°C");
            Assert.Contains(types, t => t.TypeName == "humidity" && t.DefaultUnit == "%");
        }

        [Fact]
        public async Task GetAllMeasurementTypesAsync_EmptyDb_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllMeasurementTypesAsync();

            // Assert
            Assert.NotNull(result); // wichtig!
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllStationsAsync_ReturnsAllStations()
        {
            // Arrange
            _context.Stations.Add(new() { Name = "S1" });
            _context.Stations.Add(new() { Name = "S2" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllStationsAsync();

            // Assert
            Assert.Equal(2, result.Count());

            var stations = result.ToList();
            Assert.Contains(stations, s => s.Name == "S1");
            Assert.Contains(stations, s => s.Name == "S2");
        }

        [Fact]
        public async Task GetAllStationsAsync_EmptyDb_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllStationsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExistsStationAsync_ReturnsTrueForExistingStation()
        {
            var dto = new ImportedMeasurementDto
            {
                StationName = "TestStation",
                TypeName = "humidity",
                Unit = "%",
                Value = 55,
                Status = "ok",
                TimestampUtc = DateTime.UtcNow
            };

            await _repository.AddMeasurementAsync(dto);
            await _repository.SaveChangesAsync();

            var exists = await _repository.ExistsStationAsync("TestStation");

            Assert.True(exists);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }

    }

}
