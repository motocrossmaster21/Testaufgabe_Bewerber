using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController(IWeatherDataService service, ILogger<StationsController> logger)
        : BaseApiController<StationsController>(logger)
    {
        private readonly IWeatherDataService _service = service;

        /// <summary>
        /// Returns a list of all available weather stations.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Retrieve available weather stations",
            Description = @"Retrieves a list of all available weather stations."
        )]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetStations() =>
            ExecuteSafeAsync(nameof(GetStations), async () =>
            {
                var names = await _service.GetAllStationNamesAsync().ConfigureAwait(false);
                return Ok(names);
            });
    }
}
