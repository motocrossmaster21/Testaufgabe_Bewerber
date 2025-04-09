using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherMeasurementService.Data;
using WeatherMeasurementService.Models.Dtos;
using WeatherMeasurementService.Models.ExternalApiModels;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Tests
{
    public class WeatherDataProcessingServiceTests
    {
        private readonly Mock<IWeatherDataRepository> _repoMock = new();
        private readonly Mock<ILogger<WeatherDataProcessingService>> _loggerMock = new();
        private readonly WeatherDataProcessingService _service;
        private static JsonElement JsonParse(string rawJson) => JsonDocument.Parse(rawJson).RootElement;

        public WeatherDataProcessingServiceTests()
        {
            _service = new WeatherDataProcessingService(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessWeatherDataAsync_ResponseNotOk_ShouldLogErrorAndSkip()
        {
            var response = new WeatherApiResponse { Ok = false, Message = "Error" };

            await _service.ProcessWeatherDataAsync(response);

            _repoMock.Verify(r => r.AddMeasurementAsync(It.IsAny<ImportedMeasurementDto>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ProcessWeatherDataAsync_RecordWithoutStation_ShouldBeSkipped()
        {
            var response = new WeatherApiResponse
            {
                Ok = true,
                Result = [new() { Station = null!, Timestamp = DateTime.UtcNow }]
            };

            await _service.ProcessWeatherDataAsync(response);

            _repoMock.Verify(r => r.AddMeasurementAsync(It.IsAny<ImportedMeasurementDto>()), Times.Never);
        }

        [Fact]
        public async Task ProcessWeatherDataAsync_RecordWithUnknownStation_ShouldBeSkipped()
        {
            var response = new WeatherApiResponse
            {
                Ok = true,
                Result = [new()
        {
            Station = "andere-station",
            Timestamp = DateTime.UtcNow,
            Values = new() { { "air_temperature", new WeatherApiValue { Value = JsonParse("10") } } }
        }]
            };

            await _service.ProcessWeatherDataAsync(response);

            _repoMock.Verify(r => r.AddMeasurementAsync(It.IsAny<ImportedMeasurementDto>()), Times.Never);
        }
        [Fact]
        public async Task ProcessWeatherDataAsync_TimestampCet_ShouldBeIgnored()
        {
            var response = new WeatherApiResponse
            {
                Ok = true,
                Result = [new()
        {
            Station = "mythenquai",
            Timestamp = DateTime.UtcNow,
            Values = new()
            {
                { "timestamp_cet", new WeatherApiValue { Value = JsonParse("123456") } },
                { "air_temperature", new WeatherApiValue { Value = JsonParse("10.5") } }
            }
        }]
            };

            await _service.ProcessWeatherDataAsync(response);

            _repoMock.Verify(r => r.AddMeasurementAsync(It.Is<ImportedMeasurementDto>(d => d.TypeName == "air_temperature")), Times.Once);
        }
        [Fact]
        public async Task ProcessWeatherDataAsync_InvalidNumericValue_ShouldBeSkipped()
        {
            var response = new WeatherApiResponse
            {
                Ok = true,
                Result = [new()
        {
            Station = "tiefenbrunnen",
            Timestamp = DateTime.UtcNow,
            Values = new()
            {
                { "humidity", new WeatherApiValue { Value = JsonParse("\"notANumber\"") } }
            }
        }]
            };

            await _service.ProcessWeatherDataAsync(response);

            _repoMock.Verify(r => r.AddMeasurementAsync(It.IsAny<ImportedMeasurementDto>()), Times.Never);
        }

        [Fact]
        public async Task ProcessWeatherDataAsync_ValueIsNull_ShouldBeSkipped()
        {
            var response = new WeatherApiResponse
            {
                Ok = true,
                Result = [new()
        {
            Station = "mythenquai",
            Timestamp = DateTime.UtcNow,
            Values = new()
            {
                { "precipitation", new WeatherApiValue { Value = JsonParse("null") } }
            }
        }]
            };

            await _service.ProcessWeatherDataAsync(response);

            _repoMock.Verify(r => r.AddMeasurementAsync(It.IsAny<ImportedMeasurementDto>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task ProcessWeatherDataAsync_ValidMeasurement_ShouldBeStored()
        {
            var response = new WeatherApiResponse
            {
                Ok = true,
                Result = [new()
       {
           Station = "tiefenbrunnen",
           Timestamp = DateTime.UtcNow,
           Values = new()
           {
               { "humidity", new WeatherApiValue { Value = JsonParse("55.3"), Unit = "%", Status = "ok" } }
           }
       }]
            };

            await _service.ProcessWeatherDataAsync(response);

            _repoMock.Verify(static r => r.AddMeasurementAsync(It.Is<ImportedMeasurementDto>(static dto =>
                dto.StationName == "tiefenbrunnen" &&
                dto.TypeName == "humidity" &&
                Math.Abs(dto.Value - 55.3) < 0.0001 && // Use a range for floating-point comparison  
                dto.Unit == "%" &&
                dto.Status == "ok"
            )), Times.Once);

            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

    }

}
