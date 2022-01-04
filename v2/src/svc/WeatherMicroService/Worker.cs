using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Foundant.Services;
using MassTransit;
using Foundant.Services.Contracts;

namespace WeatherMicroService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private string _serviceName;
        private IWeatherService _weatherService;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IWeatherService weatherService)
        {
            _serviceName = configuration["serviceName"];
            _logger = logger;
            _weatherService = weatherService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var temps = _weatherService.Get();
                _logger.LogInformation("Temps received ${temps}", temps);

                await Task.Delay(1200000, stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{_serviceName} Start Call");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{_serviceName} Stop Call");

            return base.StopAsync(cancellationToken);
        }
    }

    public class SendCityConsumer : IConsumer<SendCity>
    {
        readonly ILogger<SendCityConsumer> _logger;
        readonly ISendEndpointProvider _sendEndpoint;
        private IWeatherService _weatherService;

        public SendCityConsumer(ILogger<SendCityConsumer> logger, IWeatherService weatherService, ISendEndpointProvider sendEndpoint)
        {
            _logger = logger;
            _weatherService = weatherService;
            _sendEndpoint = sendEndpoint;
        }

        public async Task Consume(ConsumeContext<SendCity> context)
        {
            _logger.LogInformation($"#####################################");
            _logger.LogInformation($"Received city: {context.Message.Name}");
            _logger.LogInformation($"#####################################");

            var temps = _weatherService.Get();

            await context.Publish<SendCityForecast>(new
            {
                Name = context.Message.Name,
                Temps = temps
            });
        }
    }
}
