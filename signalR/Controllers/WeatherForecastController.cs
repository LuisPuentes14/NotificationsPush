using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalR.SignalR;

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
        private readonly IHubContext<NotificationsHub> _hubContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHubContext<NotificationsHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        [Route("{user}")]
        public IEnumerable<WeatherForecast> Get([FromQuery] string user)
        {
            if (user is not null || user != "") {
                _hubContext.Clients.Clients(user).SendAsync("ReceiveMessage", user, "esto es una pruba de mensaje");
            }
           
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                  
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}