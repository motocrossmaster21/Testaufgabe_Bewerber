using Microsoft.AspNetCore.Mvc;
using WeatherMeasurementService.Data;
using WeatherMeasurementService.Services;

namespace WeatherMeasurementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController(IWeatherDataService service, ILogger<StationsController> logger) : BaseApiController<StationsController>(logger)
    {
        private readonly IWeatherDataService _service = service;

        /// <summary>
        /// Returns a list of all available weather stations.
        /// </summary>
        [HttpGet]
        public Task<IActionResult> GetStations() =>
            ExecuteSafeAsync(nameof(GetStations), async () =>
            {
                var names = await _service.GetAllStationNamesAsync();
                return Ok(names);
            });
    }
}
