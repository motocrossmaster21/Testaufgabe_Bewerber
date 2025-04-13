using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherMeasurementService.Controllers;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Tests
{
    public class MeasurementTypeControllerIntegrationTests
    {
        private readonly Mock<IWeatherDataService> _mockService;
        private readonly Mock<ILogger<MeasurementTypeController>> _mockLogger;
        private readonly MeasurementTypeController _controller;

        public MeasurementTypeControllerIntegrationTests()
        {
            _mockService = new Mock<IWeatherDataService>();
            _mockLogger = new Mock<ILogger<MeasurementTypeController>>();
            _controller = new MeasurementTypeController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetMeasurementTypes_ReturnsOk_WithMeasurementTypeList()
        {
            // Arrange
            var expectedTypes = new List<string> { "air_temperature", "humidity", "barometric_pressure_qfe" };
            _mockService.Setup(s => s.GetAllMeasurementTypeNamesAsync())
                        .ReturnsAsync(expectedTypes);

            // Act
            var actionResult = await _controller.GetMeasurementTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnedTypes = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
            Assert.Equal(expectedTypes, returnedTypes);
        }

        [Fact]
        public async Task GetMeasurementTypes_WhenServiceThrows_ReturnsInternalServerError()
        {
            // Arrange
            var exceptionMessage = "Test exception for measurement types";
            _mockService.Setup(s => s.GetAllMeasurementTypeNamesAsync())
                        .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var actionResult = await _controller.GetMeasurementTypes();

            // Assert: Expect a 500 Internal Server Error response.
            var objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(500, objectResult.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("Internal Server Error", problemDetails.Title);
            Assert.Contains(exceptionMessage, problemDetails.Detail);
        }
    }
}
