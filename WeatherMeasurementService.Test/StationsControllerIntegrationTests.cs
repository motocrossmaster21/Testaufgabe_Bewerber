using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherMeasurementService.Controllers;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Tests
{
    public class StationsControllerIntegrationTests
    {
        private readonly Mock<IWeatherDataService> _mockService;
        private readonly Mock<ILogger<StationsController>> _mockLogger;
        private readonly StationsController _controller;

        public StationsControllerIntegrationTests()
        {
            _mockService = new Mock<IWeatherDataService>();
            _mockLogger = new Mock<ILogger<StationsController>>();
            _controller = new StationsController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetStations_ReturnsOk_WithStationList()
        {
            // Arrange
            var expectedStations = new List<string> { "tiefenbrunnen", "mythenquai" };
            _mockService.Setup(s => s.GetAllStationNamesAsync())
                        .ReturnsAsync(expectedStations);

            // Act
            var actionResult = await _controller.GetStations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnedStations = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
            Assert.Equal(expectedStations, returnedStations);
        }

        [Fact]
        public async Task GetStations_WhenServiceThrows_ReturnsInternalServerError()
        {
            // Arrange
            var exceptionMessage = "Test exception";
            _mockService.Setup(s => s.GetAllStationNamesAsync())
                        .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var actionResult = await _controller.GetStations();

            // Assert: Expect a 500 Internal Server Error response.
            var objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(500, objectResult.StatusCode);

            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("Internal Server Error", problemDetails.Title);
            Assert.Contains(exceptionMessage, problemDetails.Detail);
        }
    }
}
