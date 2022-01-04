using System;
using System.Collections.Generic;

namespace Foundant.Services.Contracts
{
    public class SendCity
    {
        public string Name { get; set; }
    }

    public class SendCityForecast
    {
        public string Name { get; set; }
        public List<(DateTime DateTime, int TemperatureC, string Summary)> Temps { get; set; }
    }
}