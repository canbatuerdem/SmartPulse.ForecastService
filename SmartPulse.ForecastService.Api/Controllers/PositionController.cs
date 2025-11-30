using Microsoft.AspNetCore.Mvc;
using SmartPulse.ForecastService.Service.DTO;
using SmartPulse.ForecastService.Service.Interfaces;

namespace SmartPulse.ForecastService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PositionController : ControllerBase
    {
        private readonly IForecastService _forecastService;

        public PositionController(IForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        /// <summary>
        /// Get total company position (sum of all plants) per hour.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCompanyPosition([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc)
        {
            var result = await _forecastService.GetCompanyPositionAsync(fromUtc, toUtc);
            return Ok(result);
        }
    }
}
