using Microsoft.AspNetCore.Mvc;
using SmartPulse.ForecastService.Service.DTO;
using SmartPulse.ForecastService.Service.Interfaces;

namespace SmartPulse.ForecastService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ForecastController : ControllerBase
    {
        private readonly IForecastService _forecastService;

        public ForecastController(IForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        /// <summary>
        /// Create or update an hourly forecast (upsert).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpsertForecast([FromBody] ForecastDto request)
        {
            try
            {
                await _forecastService.UpsertForecastAsync(request);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get forecasts between given UTC hours.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetForecasts([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc)
        {
            var result = await _forecastService.GetForecastsAsync(fromUtc, toUtc);
            return Ok(result);
        }
    }
}
