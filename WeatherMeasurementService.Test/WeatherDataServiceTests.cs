using Moq;
using WeatherMeasurementService.Data;
using WeatherMeasurementService.Models.Dtos;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Tests
{
    public class WeatherDataServiceTests
    {
        private readonly Mock<IWeatherDataRepository> _repoMock;
        private readonly WeatherDataService _service;

        public WeatherDataServiceTests()
        {
            _repoMock = new Mock<IWeatherDataRepository>(MockBehavior.Strict);
            _service = new WeatherDataService(_repoMock.Object);
        }

        private static WeatherDataQueryDto CreateValidQuery(string measurementType = "air_temperature", string station = "tiefenbrunnen") =>
           new()
           {
               MeasurementType = measurementType,
               Station = station,
               Start = DateTime.UtcNow.AddDays(-2),
               End = DateTime.UtcNow
           };

        private void SetupValidMeasurementType(string type = "air_temperature") =>
            _repoMock.Setup(r => r.ExistsMeasurementTypeAsync(type)).ReturnsAsync(true);

        private void SetupValidStation(string station = "tiefenbrunnen") =>
            _repoMock.Setup(r => r.ExistsStationAsync(station)).ReturnsAsync(true);

        private static List<WeatherDataDto> CreateSampleData(double[] values, string station = "tiefenbrunnen")
        {
            return [.. values.Select((v, i) => new WeatherDataDto
            {
                Value = v,
                StationName = station,
                MeasurementType = "air_temperature",
                Unit = "°C",
                TimestampUtc = DateTime.UtcNow.AddHours(-i)
            })];
        }

        [Fact]
        public async Task GetAverageAsync_ThrowsArgumentException_WhenStartAfterEnd()
        {
            var query = CreateValidQuery();
            query.Start = DateTime.UtcNow;
            query.End = DateTime.UtcNow.AddDays(-1);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAverageAsync(query));

            _repoMock.Verify(r => r.ExistsMeasurementTypeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAverageAsync_ThrowsArgumentException_WhenEndDateIsInFuture()
        {
            var query = CreateValidQuery();
            query.End = DateTime.UtcNow.AddDays(2);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAverageAsync(query));

            _repoMock.Verify(r => r.ExistsMeasurementTypeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAverageAsync_ThrowsMeasurementTypeNotFound_WhenInvalidType()
        {
            var query = CreateValidQuery("invalid_type");
            _repoMock.Setup(r => r.ExistsMeasurementTypeAsync("invalid_type")).ReturnsAsync(false);

            await Assert.ThrowsAsync<MeasurementTypeNotFoundException>(() => _service.GetAverageAsync(query));
        }

        [Fact]
        public async Task GetAverageAsync_ThrowsStationNotFound_WhenInvalidStation()
        {
            var query = CreateValidQuery();
            query.Station = "unknown";

            SetupValidMeasurementType();
            _repoMock.Setup(r => r.ExistsStationAsync("unknown")).ReturnsAsync(false);

            await Assert.ThrowsAsync<StationNotFoundException>(() => _service.GetAverageAsync(query));
        }

        [Fact]
        public async Task GetAverageAsync_ReturnsNull_WhenNoData()
        {
            var query = CreateValidQuery();
            SetupValidMeasurementType();
            SetupValidStation();

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(query.Station, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType))
                .ReturnsAsync([]);

            var result = await _service.GetAverageAsync(query);

            Assert.Null(result.Result);
            Assert.Empty(result.Dataset);
        }

        [Fact]
        public async Task GetAverageAsync_ReturnsCorrectAverage_WhenDataExists()
        {
            var query = CreateValidQuery();
            SetupValidMeasurementType();
            SetupValidStation();
            var data = CreateSampleData([10.0, 20.0, 30.0]);

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(query.Station, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType)).ReturnsAsync(data);

            var result = await _service.GetAverageAsync(query);

            Assert.Equal(20.0, result.Result);
            Assert.Equal(3, result.Dataset.Count);
        }

        [Fact]
        public async Task GetCountAsync_ThrowsArgumentException_WhenStartAfterEnd()
        {
            var query = CreateValidQuery();
            query.Start = DateTime.UtcNow;
            query.End = DateTime.UtcNow.AddDays(-1);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCountAsync(query));

            _repoMock.Verify(r => r.ExistsMeasurementTypeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetCountAsync_ThrowsArgumentException_WhenEndDateIsInFuture()
        {
            var query = CreateValidQuery();
            query.End = DateTime.UtcNow.AddDays(2);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCountAsync(query));

            _repoMock.Verify(r => r.ExistsMeasurementTypeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetCountAsync_ThrowsMeasurementTypeNotFound_WhenInvalidType()
        {
            var query = CreateValidQuery("invalid_type");
            _repoMock.Setup(r => r.ExistsMeasurementTypeAsync("invalid_type")).ReturnsAsync(false);

            await Assert.ThrowsAsync<MeasurementTypeNotFoundException>(() => _service.GetCountAsync(query));
        }

        [Fact]
        public async Task GetCountAsync_ThrowsStationNotFound_WhenInvalidStation()
        {
            var query = CreateValidQuery();
            query.Station = "unknown";

            SetupValidMeasurementType();
            _repoMock.Setup(r => r.ExistsStationAsync("unknown")).ReturnsAsync(false);

            await Assert.ThrowsAsync<StationNotFoundException>(() => _service.GetCountAsync(query));
        }

        [Fact]
        public async Task GetCountAsync_ReturnsNull_WhenNoData()
        {
            var query = CreateValidQuery();
            SetupValidMeasurementType();
            SetupValidStation();

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(query.Station, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType))
                .ReturnsAsync([]);

            var result = await _service.GetCountAsync(query);

            Assert.Empty(result.Dataset);
        }

        [Fact]
        public async Task GetCountAsync_ReturnsCorrectCount_WhenDataExists()
        {
            var query = CreateValidQuery();
            SetupValidMeasurementType();
            SetupValidStation();
            var data = CreateSampleData([12.5, 13.1]);

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(query.Station, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType)).ReturnsAsync(data);

            var result = await _service.GetCountAsync(query);

            Assert.Equal(2, result.Result);
            Assert.Equal(2, result.Dataset.Count);
        }

        [Fact]
        public async Task GetHighestAsync_ThrowsArgumentException_WhenStartAfterEnd()
        {
            var query = CreateValidQuery();
            query.Start = DateTime.UtcNow;
            query.End = DateTime.UtcNow.AddDays(-1);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetHighestAsync(query));

            _repoMock.Verify(r => r.ExistsMeasurementTypeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetHighestAsync_ThrowsArgumentException_WhenEndDateIsInFuture()
        {
            var query = CreateValidQuery();
            query.End = DateTime.UtcNow.AddDays(2);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetHighestAsync(query));

            _repoMock.Verify(r => r.ExistsMeasurementTypeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetHighestAsync_ThrowsMeasurementTypeNotFound_WhenInvalidType()
        {
            var query = CreateValidQuery("invalid_type");
            _repoMock.Setup(r => r.ExistsMeasurementTypeAsync("invalid_type")).ReturnsAsync(false);

            await Assert.ThrowsAsync<MeasurementTypeNotFoundException>(() => _service.GetHighestAsync(query));
        }

        [Fact]
        public async Task GetHighestAsync_ThrowsStationNotFound_WhenInvalidStation()
        {
            var query = CreateValidQuery();
            query.Station = "unknown";

            SetupValidMeasurementType();
            _repoMock.Setup(r => r.ExistsStationAsync("unknown")).ReturnsAsync(false);

            await Assert.ThrowsAsync<StationNotFoundException>(() => _service.GetHighestAsync(query));
        }

        [Fact]
        public async Task GetHighestAsync_ReturnsNull_WhenNoData()
        {
            var query = CreateValidQuery();
            SetupValidMeasurementType();
            SetupValidStation();

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(query.Station, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType))
                .ReturnsAsync([]);

            var result = await _service.GetHighestAsync(query);

            Assert.Null(result.Result);
            Assert.Empty(result.Dataset);
        }

        [Fact]
        public async Task GetHighestAsync_ReturnsHighestMeasurement_WhenDataExists()
        {
            var query = CreateValidQuery();
            SetupValidMeasurementType();
            SetupValidStation();
            var data = CreateSampleData([18.7, 12.3, 15.0]);

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(query.Station, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType)).ReturnsAsync(data);

            var result = await _service.GetHighestAsync(query);

            Assert.NotNull(result.Result);
            Assert.Equal(18.7, result.Result!.Value);
        }

        [Fact]
        public async Task GetLowestAsync_ThrowsArgumentException_WhenStartAfterEnd()
        {
            var query = CreateValidQuery();
            query.Start = DateTime.UtcNow;
            query.End = DateTime.UtcNow.AddDays(-1);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetLowestAsync(query));

            _repoMock.Verify(r => r.ExistsMeasurementTypeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetLowestAsync_ThrowsArgumentException_WhenEndDateIsInFuture()
        {
            var query = CreateValidQuery();
            query.End = DateTime.UtcNow.AddDays(2);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetLowestAsync(query));

            _repoMock.Verify(r => r.ExistsMeasurementTypeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetLowestAsync_ThrowsMeasurementTypeNotFound_WhenInvalidType()
        {
            var query = CreateValidQuery("invalid_type");
            _repoMock.Setup(r => r.ExistsMeasurementTypeAsync("invalid_type")).ReturnsAsync(false);

            await Assert.ThrowsAsync<MeasurementTypeNotFoundException>(() => _service.GetLowestAsync(query));
        }

        [Fact]
        public async Task GetLowestAsync_ThrowsStationNotFound_WhenInvalidStation()
        {
            var query = CreateValidQuery();
            query.Station = "unknown";

            SetupValidMeasurementType();
            _repoMock.Setup(r => r.ExistsStationAsync("unknown")).ReturnsAsync(false);

            await Assert.ThrowsAsync<StationNotFoundException>(() => _service.GetLowestAsync(query));
        }

        [Fact]
        public async Task GetLowestAsync_ReturnsNull_WhenNoData()
        {
            var query = CreateValidQuery();
            SetupValidMeasurementType();
            SetupValidStation();

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(query.Station, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType))
                .ReturnsAsync([]);

            var result = await _service.GetLowestAsync(query);

            Assert.Null(result.Result);
            Assert.Empty(result.Dataset);
        }

        [Fact]
        public async Task GetLowestAsync_ReturnsLowestMeasurement_WhenDataExists()
        {
            var query = CreateValidQuery();
            SetupValidMeasurementType();
            SetupValidStation();
            var data = CreateSampleData([7.2, 5.0, 10.0]);

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(query.Station, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType)).ReturnsAsync(data);

            var result = await _service.GetLowestAsync(query);

            Assert.NotNull(result.Result);
            Assert.Equal(5.0, result.Result!.Value);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsFilteredAndFullDataset()
        {
            var query = CreateValidQuery();
            SetupValidMeasurementType();
            SetupValidStation();
            var dataset = CreateSampleData([10.0, 20.0, 30.0]);

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(null, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType)).ReturnsAsync(dataset);

            var result = await _service.GetAllAsync(query);

            Assert.Equal(3, result.Result?.Count);
            Assert.Equal(3, result.Dataset.Count);
        }

        [Fact]
        public async Task GetAllAsync_WithStation_FiltersCorrectly()
        {
            // Arrange
            var query = CreateValidQuery();
            query.Station = "mythenquai";
            SetupValidMeasurementType();
            SetupValidStation();

            var dataset = new List<WeatherDataDto>
            {
                new()
                {
                    StationName = "mythenquai",
                    MeasurementType = query.MeasurementType,
                    Value = 10.0,
                    Unit = "°C",
                    TimestampUtc = DateTime.UtcNow.AddHours(-2)
                },
                new()
                {
                    StationName = "tiefenbrunnen",
                    MeasurementType = query.MeasurementType,
                    Value = 20.0,
                    Unit = "°C",
                    TimestampUtc = DateTime.UtcNow.AddHours(-1)
                },
                new()
                {
                    StationName = "mythenquai",
                    MeasurementType = query.MeasurementType,
                    Value = 30.0,
                    Unit = "°C",
                    TimestampUtc = DateTime.UtcNow
                }
            };


            _repoMock.Setup(r => r.GetAllMeasurementsAsync(null, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType))
                     .ReturnsAsync(dataset);
            _repoMock.Setup(r => r.ExistsStationAsync(query.Station)).ReturnsAsync(true);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            Assert.Equal(2, result.Result?.Count);
            Assert.Equal(3, result.Dataset.Count);
            Assert.NotNull(result.Result);
            Assert.All(result.Result, x => Assert.Equal("mythenquai", x.StationName));
        }

        [Fact]
        public async Task GetAllAsync_WithStation_NoMatch_ReturnsEmptyFiltered()
        {
            // Arrange
            var query = CreateValidQuery();
            query.Station = "unknown";
            SetupValidMeasurementType();
            SetupValidStation();

            var dataset = CreateSampleData([10.0, 20.0, 30.0], station: "tiefenbrunnen");

            _repoMock.Setup(r => r.GetAllMeasurementsAsync(null, It.IsAny<DateTime>(), It.IsAny<DateTime>(), query.MeasurementType))
                     .ReturnsAsync(dataset);
            _repoMock.Setup(r => r.ExistsStationAsync(query.Station)).ReturnsAsync(true); // override true

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            Assert.NotNull(result.Result);
            Assert.Empty(result.Result);
            Assert.Equal(3, result.Dataset.Count);
        }

        [Fact]
        public async Task GetAllStationNamesAsync_ReturnsAllNames()
        {
            var stationDtos = new List<StationDto>
            {
                new() { Name = "tiefenbrunnen" },
                new() { Name = "mythenquai" }
            };

            _repoMock.Setup(r => r.GetAllStationsAsync()).ReturnsAsync(stationDtos);

            var result = await _service.GetAllStationNamesAsync();

            Assert.Contains("tiefenbrunnen", result);
            Assert.Contains("mythenquai", result);
        }

        [Fact]
        public async Task GetAllMeasurementTypeNamesAsync_ReturnsAllNames()
        {
            var typeDtos = new List<MeasurementTypeDto>
            {
                new() { TypeName = "air_temperature", DefaultUnit = "°C" },
                new() { TypeName = "humidity", DefaultUnit = "%" }
            };

            _repoMock.Setup(r => r.GetAllMeasurementTypesAsync()).ReturnsAsync(typeDtos);

            var result = await _service.GetAllMeasurementTypeNamesAsync();

            Assert.Contains("air_temperature", result);
            Assert.Contains("humidity", result);
        }
    }
}

