using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Tests
{
    public class ExternalWeatherApiClientTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<ILogger<ExternalWeatherApiClient>> _loggerMock;
        private readonly ExternalWeatherApiClient _client;

        public ExternalWeatherApiClientTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            _loggerMock = new Mock<ILogger<ExternalWeatherApiClient>>();
            var httpClient = _mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri("https://fake.api/");
            _client = new ExternalWeatherApiClient(httpClient, _loggerMock.Object);
        }

        [Fact]
        public async Task FetchWeatherDataAsync_SuccessfulResponse_ReturnsParsedObject()
        {
            var json = """
        {
            "ok": true,
            "result": [],
            "message": null
        }
        """;

            _mockHttp.When("*measurements/*")
                     .Respond("application/json", json);

            var result = await _client.FetchWeatherDataAsync("mythenquai", DateTime.Today, DateTime.Today, "asc", 100, 0);

            Assert.NotNull(result);
            Assert.True(result.Ok);
        }

        [Fact]
        public async Task FetchWeatherDataAsync_FullValidResponse_ReturnsParsedObject()
        {
            var json = """
            {
              "ok": true,
              "message": null,
              "total_count": 100,
              "row_count": 1,
              "result": [
                {
                  "station": "mythenquai",
                  "timestamp": "2024-04-09T12:00:00Z",
                  "values": {
                    "air_temperature": {
                      "value": 13.5,
                      "unit": "°C",
                      "status": "ok"
                    },
                    "precipitation": {
                      "value": null,
                      "unit": "mm",
                      "status": "ok"
                    }
                  }
                }
              ]
            }
            """;

            _mockHttp.When("*measurements/*")
                     .Respond("application/json", json);

            var result = await _client.FetchWeatherDataAsync("mythenquai", DateTime.Today, DateTime.Today, "asc", 100, 0);

            Assert.NotNull(result);
            Assert.True(result.Ok);
            Assert.Single(result.Result);
            var record = result.Result[0];
            Assert.Equal("mythenquai", record.Station);
            Assert.True(record.Values.ContainsKey("air_temperature"));
            Assert.True(record.Values.ContainsKey("precipitation"));
            Assert.Equal(JsonValueKind.Null, record.Values["precipitation"].Value.ValueKind);
        }

        [Fact]
        public async Task FetchWeatherDataAsync_HttpError_ReturnsNullAndLogsError()
        {
            _mockHttp.When("*measurements/*")
                     .Respond(HttpStatusCode.InternalServerError);

            var result = await _client.FetchWeatherDataAsync("tiefenbrunnen", DateTime.Today, DateTime.Today, "desc", 100, 0);

            Assert.Null(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch data")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task FetchWeatherDataAsync_ResponseOkFalse_LogsWarningAndReturnsNull()
        {
            var json = """
        {
            "ok": false,
            "message": "Something went wrong"
        }
        """;

            _mockHttp.When("*measurements/*")
                     .Respond("application/json", json);

            var result = await _client.FetchWeatherDataAsync("tiefenbrunnen", DateTime.Today, DateTime.Today, "desc", 100, 0);

            Assert.Null(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("'ok': false")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task FetchWeatherDataAsync_InvalidJson_ReturnsNullAndLogsError()
        {
            var invalidJson = "{ not valid json }";

            _mockHttp.When("*measurements/*")
                     .Respond("application/json", invalidJson);

            var result = await _client.FetchWeatherDataAsync("tiefenbrunnen", DateTime.Today, DateTime.Today, "desc", 100, 0);

            Assert.Null(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exception during FetchWeatherDataAsync")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

}
