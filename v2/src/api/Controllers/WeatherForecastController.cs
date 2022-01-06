using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundant.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Foundant.Services.Contracts;

namespace Foundant.Core.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IWeatherService weatherService;
        readonly ISendEndpointProvider _sendEndpoint;
        private readonly ILogger _logger;

        //public WeatherForecastController(ILogger logger, IWeatherService weatherService, ISendEndpointProvider sendEndpoint)
        public WeatherForecastController(ILogger logger)
        {
            _logger = logger;
            //_sendEndpoint = sendEndpoint;
            this.weatherService = weatherService;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.Information("Calling Weather Service");
            _logger.Debug("Example Debug Message");
            _logger.Error("Example Error Message");
            _logger.Fatal("Example Fatal Message");

            return new List<WeatherForecast>();

            var temps = weatherService.Get();

            //_logger.Information("Temps received ${temps}", temps);
                        
            //return temps.Select(t => new WeatherForecast
            //{
            //    Date = t.DateTime,
            //    TemperatureC = t.TemperatureC,
            //    Summary = t.Summary
            //}).ToList();
        }

        [HttpPost]
        public async Task<ActionResult> Post(string cityName)
        {
            _logger.Information($"Submitting message for {cityName}");

            //var endpoint = await _sendEndpoint.GetSendEndpoint(new System.Uri("queue:send-city"));

            //await endpoint.Send(new SendCity { Name = cityName });

            return Ok();
        }
    }

    public class SendCityForecastConsumer : IConsumer<SendCityForecast>
    {
        readonly ILogger _logger;

        public SendCityForecastConsumer(ILogger logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<SendCityForecast> context)
        {
            _logger.Information($"#####################################");
            _logger.Information($"Received forecast:");
            _logger.Information($"City: {context.Message.Name}");
            foreach (var day in context.Message.Temps)
            {
                var forecast = new WeatherForecast
                {
                    Date = day.DateTime,
                    TemperatureC = day.TemperatureC,
                    Summary = day.Summary
                };

                _logger.Information($"Forecast: {forecast.Date} : {forecast.TemperatureF} ");
            }
            _logger.Information($"#####################################");

            return Task.CompletedTask;
        }
    }
}
