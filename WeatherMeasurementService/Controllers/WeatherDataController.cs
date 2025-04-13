using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherMeasurementService.Models.Dtos;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherDataController(IWeatherDataService service, ILogger<WeatherDataController> logger)
        : BaseApiController<WeatherDataController>(logger)
    {
        private readonly IWeatherDataService _service = service;

        [HttpGet("highest")]
        [SwaggerOperation(
            Summary = "Retrieve highest measurement",
            Description = @"Retrieves the highest measurement value based on the provided query parameters 
(MeasurementType, optional Station, and date range defined by Start and End).
Additionally, the complete dataset is also included in the response."
        )]
        [ProducesResponseType(typeof(ResponseWeatherDataDto<WeatherDataDto?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetHighestAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetHighestAsync), async () =>
                Ok(await _service.GetHighestAsync(query).ConfigureAwait(false)));

        [HttpGet("lowest")]
        [SwaggerOperation(
            Summary = "Retrieve lowest measurement",
            Description = @"Retrieves the lowest measurement value based on the provided query parameters 
(MeasurementType, optional Station, and date range defined by Start and End).
Additionally, the complete dataset is also included in the response."
        )]
        [ProducesResponseType(typeof(ResponseWeatherDataDto<WeatherDataDto?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetLowestAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetLowestAsync), async () =>
                Ok(await _service.GetLowestAsync(query).ConfigureAwait(false)));

        [HttpGet("average")]
        [SwaggerOperation(
            Summary = "Retrieve average measurement",
            Description = @"Retrieves the average measurement value based on the provided query parameters 
(MeasurementType, optional Station, and date range defined by Start and End).
Additionally, the complete dataset is also included in the response."
        )]
        [ProducesResponseType(typeof(ResponseWeatherDataDto<double?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetAverageAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetAverageAsync), async () =>
                Ok(await _service.GetAverageAsync(query).ConfigureAwait(false)));

        [HttpGet("count")]
        [SwaggerOperation(
            Summary = "Retrieve measurement count",
            Description = @"Retrieves the count of measurements based on the provided query parameters 
(MeasurementType, optional Station, and date range defined by Start and End).
Additionally, the complete dataset is also included in the response."
        )]
        [ProducesResponseType(typeof(ResponseWeatherDataDto<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetCountAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetCountAsync), async () =>
                Ok(await _service.GetCountAsync(query).ConfigureAwait(false)));

        [HttpGet]
        [SwaggerOperation(
            Summary = "Retrieve all measurements",
            Description = @"Retrieves all measurement data based on the provided query parameters 
(MeasurementType, optional Station, and date range defined by Start and End).
Additionally, the complete dataset is also included in the response."
        )]
        [ProducesResponseType(typeof(ResponseWeatherDataDto<List<WeatherDataDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetAllAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetAllAsync), async () =>
                Ok(await _service.GetAllAsync(query).ConfigureAwait(false)));
    }
}
