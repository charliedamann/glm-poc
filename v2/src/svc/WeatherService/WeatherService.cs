using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Foundant.Services
{
    public interface IWeatherService
    {
        List<(DateTime DateTime, int TemperatureC, string Summary)> Get();
    }

    public class WeatherService : IWeatherService
    {
        private ILogger<WeatherService> _logger;

        public WeatherService(ILogger<WeatherService> logger)
        {
            _logger = logger;
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public List<(DateTime DateTime, int TemperatureC, string Summary)> Get()
        {
            _logger.LogInformation("Calculating temperatures");
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .AsEnumerable()
            .Select(w => (DateTime: w.Date, TemperatureC: w.TemperatureC, Summary: w.Summary))
            .ToList();
        }
    }
}
