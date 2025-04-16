using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeasurementTypeController(IWeatherDataService service,ILogger<MeasurementTypeController> logger)
        : BaseApiController<MeasurementTypeController>(logger)
    {
        private readonly IWeatherDataService _service = service;

        /// <summary>
        /// Returns a list of all available measurement types.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Retrieve available measurement types",
            Description = @"Retrieves a list of all available measurement types."
        )]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetMeasurementTypes() =>
            ExecuteSafeAsync(nameof(GetMeasurementTypes), async () =>
            {
                var names = await _service.GetAllMeasurementTypeNamesAsync().ConfigureAwait(false);
                return Ok(names);
            });
    }
}
