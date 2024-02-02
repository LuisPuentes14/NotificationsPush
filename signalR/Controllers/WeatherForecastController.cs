using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using signalR.Utils.JWT;

namespace signalR.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IJWT _jWT;


        public WeatherForecastController(ILogger<WeatherForecastController> logger, IJWT jWT)
        {
            _logger = logger;
            _jWT = jWT;

        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {

            string token = _jWT.generateToken(new Models.User()
            {
                name = "alejandro",
                email = "alejandropuentes@gmail.com",
                login = "LAPA14"
            });

            return Ok(token);
        }


        [HttpPost(Name = "testAuthorize")]
        [Authorize]
        public IActionResult testAuthorize()
        {
            string token = "alejandro eres el mejor";
            return Ok(token);
        }
    }
}