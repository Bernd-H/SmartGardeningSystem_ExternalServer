using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace ExternalServer.RestAPI.Controllers.BasestationControllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeatherController : ControllerBase {

        private IWeatherRepository WeatherRepository;

        private ILogger Logger;

        public WeatherController(ILoggerService loggerService, IWeatherRepository weatherRepository) {
            Logger = loggerService.GetLogger<WeatherController>();
            WeatherRepository = weatherRepository;
        }

        // GET api/<WeatherController>
        [HttpGet("{location}")]
        public ActionResult<WeatherForecast> GetWeather(string location) {
            if (ControllerHelperClass.CallerIsBasestation(HttpContext)) {
                if (string.IsNullOrEmpty(location)) {
                    return BadRequest();
                }

                Logger.Info($"[GetWeather]Weather forecast requested.");
                return WeatherRepository.GetCurrentWeatherPredictions(location).Result;
            }

            return Unauthorized();
        }
    }
}
