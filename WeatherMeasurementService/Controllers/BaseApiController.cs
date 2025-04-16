using Microsoft.AspNetCore.Mvc;
using WeatherMeasurementService.Models.Dtos;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Controllers
{
    public abstract class BaseApiController<T>(ILogger<T> logger) : ControllerBase
    {
        protected readonly ILogger<T> _logger = logger;

        protected Task<IActionResult> ExecuteSafeAsync(
            WeatherDataQueryDto query,
            string operation,
            Func<Task<IActionResult>> action)
        {
            _logger.LogInformation("Received {Operation} request: MeasurementType={Type}, Start={Start}, End={End}, Station={Station}",
                operation, query.MeasurementType, query.Start, query.End, query.Station ?? "any");

            return ExecuteSafeInternalAsync(operation, action, query);
        }

        protected Task<IActionResult> ExecuteSafeAsync(
            string operation,
            Func<Task<IActionResult>> action)
        {
            _logger.LogInformation("Received {Operation} request", operation);

            return ExecuteSafeInternalAsync(operation, action);
        }

        private async Task<IActionResult> ExecuteSafeInternalAsync(
            string operation,
            Func<Task<IActionResult>> action,
            WeatherDataQueryDto? query = null)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (MeasurementTypeNotFoundException ex) when (query is not null)
            {
                _logger.LogWarning(ex, "Measurement type '{Type}' not found", query.MeasurementType);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Measurement Type Not Found",
                    Detail = ex.Message
                });
            }
            catch (StationNotFoundException ex) when (query is not null)
            {
                _logger.LogWarning(ex, "Station '{Station}' not found", query.Station);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Station Not Found",
                    Detail = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request for operation '{Operation}'", operation);
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in operation '{Operation}'", operation);
                return StatusCode(500, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = $"An unexpected error occurred. {ex.Message}"
                });
            }
        }
    }
}
