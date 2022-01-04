using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using Foundant.Services;
using MassTransit;

namespace WeatherMicroService
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .CreateBootstrapLogger();
            try
            {
                Log.Information("Starting WeatherMicroService stack.");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        static bool? _isRunningInContainer;
        static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services))
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IWeatherService, WeatherService>();

                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        x.AddConsumer<SendCityConsumer>();

                        x.UsingRabbitMq((ctx, cfg) =>
                        {
                            cfg.AutoStart = true;
                            cfg.ConfigureEndpoints(ctx);

                            if (IsRunningInContainer)
                            {
                                cfg.Host("rabbitmq");
                            }
                            else
                            {
                                cfg.Host(new Uri(hostContext.Configuration["RabbitMQ:ConnectionString"]), h =>
                                    {
                                        h.Username(hostContext.Configuration["RabbitMQ:Username"]);
                                        h.Password(hostContext.Configuration["RabbitMQ:Password"]);
                                    });
                            }
                        });
                    });

                    services.AddMassTransitHostedService();
                });
    }
}
