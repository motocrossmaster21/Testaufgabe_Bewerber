using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WeatherMeasurementService.Data;
using WeatherMeasurementService.Models.Dtos;

namespace WeatherMeasurementService.Tests
{
    [Collection("Integration Tests")]
    public class WeatherDataControllerIntegrationTests : IAsyncLifetime
    {
        private readonly InitDbHelper _fixture;
        private readonly HttpClient _client;

        public WeatherDataControllerIntegrationTests(InitDbHelper fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        // Is executed before each fact
        public async Task InitializeAsync()
        {
            await _fixture.ClearDatabaseAsync();
        }

        // Executed after each fact
        public async Task DisposeAsync()
        {
            await _fixture.ClearDatabaseAsync();
        }

        // Helper for filling the database.
        private async Task SeedMeasurementAsync(ImportedMeasurementDto dto)
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IWeatherDataRepository>();
            await repo.AddMeasurementAsync(dto);
            await repo.SaveChangesAsync();
        }

        [Fact]
        public async Task GetHighestAsync_ReturnsHighestMeasurement()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto1 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 18.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            };
            var dto2 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 22.0, // Higher value
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(1) // Different timestamp
            };
            var dto3 = new ImportedMeasurementDto
            {
                StationName = "mythenquai", // Station not included
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 99.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(2) // Different timestamp
            };

            await SeedMeasurementAsync(dto1);
            await SeedMeasurementAsync(dto2);
            await SeedMeasurementAsync(dto3);

            var url = $"/api/weatherdata/highest?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";

            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<WeatherDataDto?>>();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.Equal(22.0, result.Result!.Value);
            Assert.Equal(2, result.Dataset.Count);
        }

        [Fact]
        public async Task GetHighestAsync_WithSameValue_ReturnsHighestMeasurement()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto1 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 18.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            };
            var dto2 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 22.0, // Higher value
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(1) // Different timestamp
            };
            var dto3 = new ImportedMeasurementDto
            {
                StationName = "mythenquai", // Station not included
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 22.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(2) // Different timestamp
            };

            await SeedMeasurementAsync(dto1);
            await SeedMeasurementAsync(dto2);
            await SeedMeasurementAsync(dto3);

            var url = $"/api/weatherdata/highest?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}";

            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<WeatherDataDto?>>();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.Equal(22.0, result.Result!.Value);
            Assert.Equal(3, result.Dataset.Count);
        }

        [Fact]
        public async Task GetLowestAsync_ReturnsLowestMeasurement()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto1 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "humidity",
                Unit = "%",
                Value = 40.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            };
            var dto2 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "humidity",
                Unit = "%",
                Value = 30.0, // Lower value
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(1) // Different timestamp
            };
            var dto3 = new ImportedMeasurementDto
            {
                StationName = "mythenquai", // Station not included
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 12.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(2) // Different timestamp
            };

            await SeedMeasurementAsync(dto1);
            await SeedMeasurementAsync(dto2);
            await SeedMeasurementAsync(dto3);

            var url = $"/api/weatherdata/lowest?measurementType=humidity" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";

            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<WeatherDataDto?>>();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.Equal(30.0, result.Result!.Value);
            Assert.Equal(2, result.Dataset.Count);
        }

        [Fact]
        public async Task GetAverageAsync_ReturnsCorrectAverage()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto1 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 10.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            };
            var dto2 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 20.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(1) // Different timestamp
            };
            var dto3 = new ImportedMeasurementDto
            {
                StationName = "mythenquai", // Station not included
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 12.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(2) // Different timestamp
            };

            await SeedMeasurementAsync(dto1);
            await SeedMeasurementAsync(dto2);
            await SeedMeasurementAsync(dto3);

            var url = $"/api/weatherdata/average?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";

            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<double?>>();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.Equal(15.0, result.Result!.Value, precision: 1); // Two measured values: 10 and 20 => average = 15
            Assert.Equal(2, result.Dataset.Count);
        }

        [Fact]
        public async Task GetCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto1 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "humidity",
                Unit = "%",
                Value = 60,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            };
            var dto2 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "humidity",
                Unit = "%",
                Value = 65,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(1) // Different timestamp
            };
            var dto3 = new ImportedMeasurementDto
            {
                StationName = "mythenquai", // Station not included
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 12.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(2) // Different timestamp
            };

            await SeedMeasurementAsync(dto1);
            await SeedMeasurementAsync(dto2);
            await SeedMeasurementAsync(dto3);

            var url = $"/api/weatherdata/count?measurementType=humidity" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";

            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<int>>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Result);
            Assert.Equal(2, result.Dataset.Count);
        }


        [Fact]
        public async Task GetAllAsync_ReturnsAllMeasurements_AllStations()
        {
            // Arrange
            var now = DateTime.UtcNow;
            // Two measurements, at different stations, but the same type.
            var dto1 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 12.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            };
            var dto2 = new ImportedMeasurementDto
            {
                StationName = "mythenquai",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 18.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(1) // Different timestamp
            };
            var dto3 = new ImportedMeasurementDto
            {
                StationName = "mythenquai",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 12.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(2) // Different timestamp
            };

            await SeedMeasurementAsync(dto1);
            await SeedMeasurementAsync(dto2);
            await SeedMeasurementAsync(dto3);

            // Since station=zero is set, all measurements of the type should be supplied.
            var url = $"/api/weatherdata/all?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}";

            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<List<WeatherDataDto>>>();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.Equal(3, result.Result.Count);
            Assert.Equal(3, result.Dataset.Count);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyDataset()
        {
            // Arrange: keine Daten in der DB
            var now = DateTime.UtcNow;
            var url = $"/api/weatherdata/all?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}";
            // Act
            var response = await _client.GetAsync(url);

            // Assert: Expect 404 NotFound because the measurement type is missing.
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAverageAsync_WithEmptyDatabase_ReturnsNull()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var url = $"/api/weatherdata/average?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);

            // Assert: Expect 404 NotFound because the measurement type or station is missing.
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetHighestAsync_WithEmptyDatabase_ReturnsNull()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var url = $"/api/weatherdata/highest?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);

            // Assert: Expect 404 NotFound because the measurement type or station is missing.
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetLowestAsync_WithEmptyDatabase_ReturnsNull()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var url = $"/api/weatherdata/lowest?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);

            // Assert: Expect 404 NotFound because the measurement type or station is missing.
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCountAsync_WithEmptyDatabase_ReturnsNull()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var url = $"/api/weatherdata/count?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);

            // Assert: Expect 404 NotFound because the measurement type or station is missing.
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllAsync_FilterByStation_ReturnsOnlyMeasurementsForThatStation()
        {
            // Arrange
            var now = DateTime.UtcNow;
            // Seed: Two measurements, different stations, same type.
            var dto1 = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 14.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-2)
            };
            var dto2 = new ImportedMeasurementDto
            {
                StationName = "mythenquai",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 16.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            };
            var dto3 = new ImportedMeasurementDto
            {
                StationName = "mythenquai",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 12.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1).AddMinutes(2) // Different timestamp
            };
            await SeedMeasurementAsync(dto1);
            await SeedMeasurementAsync(dto2);
            await SeedMeasurementAsync(dto3);

            // Query with station filter = tiefenbrunnen
            var url = $"/api/weatherdata/all?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<List<WeatherDataDto>>>();

            // Assert: Only the measurement from tiefenbrunnen is supplied.
            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.Single(result.Result);
            Assert.Equal(3, result.Dataset.Count);
            Assert.Equal("tiefenbrunnen", result.Dataset.First().StationName);
        }

        [Fact]
        public async Task GetAllAsync_IncludesBoundaryDates()
        {
            // Arrange
            var now = DateTime.UtcNow;
            // Seed: Measurement exactly at midnight on the start date and on the end date.
            var startDate = now.Date;
            var endDate = now.Date.AddDays(1).AddTicks(-1);
            var dtoStart = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 12.0,
                Status = "ok",
                TimestampUtc = startDate // exact start date
            };
            var dtoEnd = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 18.0,
                Status = "ok",
                TimestampUtc = endDate // exact end date
            };

            await SeedMeasurementAsync(dtoStart);
            await SeedMeasurementAsync(dtoEnd);

            var url = $"/api/weatherdata/all?measurementType=air_temperature" +
                      $"&start={startDate:yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<List<WeatherDataDto>>>();

            // Assert: Both measurements should be included.
            Assert.NotNull(result);
            Assert.Equal(2, result.Dataset.Count);
        }

        [Fact]
        public async Task GetAllAsync_InvalidDateRange_ReturnsBadRequest()
        {
            // Arrange
            var now = DateTime.UtcNow;
            // Set start date after the end date
            var url = $"/api/weatherdata/all?measurementType=air_temperature" +
                      $"&start={now.AddDays(1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);
            // Assert: Should receive 400 Bad Request
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAllAsync_WithNonexistentMeasurementType_ReturnsNotFound()
        {
            // Arrange: No measurement with the type "nonexistent" will be in the DB.
            var now = DateTime.UtcNow;
            var url = $"/api/weatherdata/all?measurementType=nonexistent" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);
            // Assert: Should receive 400 Bad Request
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAverageAsync_SingleMeasurement_ReturnsThatValue()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "air_temperature",
                Unit = "°C",
                Value = 20.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            };
            await SeedMeasurementAsync(dto);

            var url = $"/api/weatherdata/average?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<double?>>();
            // Assert: Average is the only value
            Assert.NotNull(result);
            Assert.Equal(20.0, result.Result);
        }

        [Fact]
        public async Task GetCountAsync_ReturnsCorrectCount_AfterDuplicateInsertion()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new ImportedMeasurementDto
            {
                StationName = "tiefenbrunnen",
                TypeName = "humidity",
                Unit = "%",
                Value = 55.0,
                Status = "ok",
                TimestampUtc = now.AddHours(-1)
            };
            // Try to save the same measurement twice (same timestamp, same date)
            await SeedMeasurementAsync(dto);
            await SeedMeasurementAsync(dto); // Duplicate

            var url = $"/api/weatherdata/count?measurementType=humidity" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResponseWeatherDataDto<int>>();
            // Assert: Duplicates should not be saved. Only 1 measurement
            Assert.NotNull(result);
            Assert.Equal(1, result.Result);
        }


        [Fact]
        public async Task GetHighestAsync_FutureEndDate_ReturnsBadRequest()
        {
            // Arrange: End date in the future
            var now = DateTime.UtcNow;
            var url = $"/api/weatherdata/highest?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now.AddDays(1):yyyy-MM-dd}&station=tiefenbrunnen";
            // Act
            var response = await _client.GetAsync(url);
            // Assert: Should receive 400 Bad Request
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetHighestAsync_NonexistentStation_ReturnsNotFound()
        {
            // Arrange: Station does not exist
            var now = DateTime.UtcNow;
            var url = $"/api/weatherdata/highest?measurementType=air_temperature" +
                      $"&start={now.AddDays(-1):yyyy-MM-dd}&end={now:yyyy-MM-dd}&station=NonexistentStation";
            // Act
            var response = await _client.GetAsync(url);
            // Assert: Should receive 400 Bad Request
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

    }
}
