using Microsoft.AspNetCore.Mvc;
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
        public Task<IActionResult> GetHighestAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetHighestAsync), async () => Ok(await _service.GetHighestAsync(query).ConfigureAwait(false)));

        [HttpGet("lowest")]
        public Task<IActionResult> GetLowestAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetLowestAsync), async () => Ok(await _service.GetLowestAsync(query).ConfigureAwait(false)));

        [HttpGet("average")]
        public Task<IActionResult> GetAverageAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetAverageAsync), async () => Ok(await _service.GetAverageAsync(query).ConfigureAwait(false)));

        [HttpGet("count")]
        public Task<IActionResult> GetCountAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetCountAsync), async () => Ok(await _service.GetCountAsync(query).ConfigureAwait(false)));

        [HttpGet("all")]
        public Task<IActionResult> GetAllAsync([FromQuery] WeatherDataQueryDto query) =>
            ExecuteSafeAsync(query, nameof(GetAllAsync), async () => Ok(await _service.GetAllAsync(query).ConfigureAwait(false)));
    }
}
