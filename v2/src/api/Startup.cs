using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Foundant.Services;
using MassTransit;
using System;
using Foundant.Core.Api.Controllers;
using Serilog;

namespace Foundant.Core.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        static bool? _isRunningInContainer;

        // DOTNET_RUNNING_IN_CONTAINER is an ENV variable set in the MS docker images... very handy
        // https://www.hanselman.com/blog/detecting-that-a-net-core-app-is-running-in-a-docker-container-and-skippablefacts-in-xunit

        static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMassTransit(x =>
            //{
            //    x.SetKebabCaseEndpointNameFormatter();

            //    x.AddConsumer<SendCityForecastConsumer>();

            //    x.UsingRabbitMq((ctx, cfg) =>
            //    {
            //        cfg.AutoStart = true;
            //        cfg.ConfigureEndpoints(ctx);

            //        if (IsRunningInContainer)
            //        {
            //            cfg.Host("rabbitmq");
            //        }
            //        else
            //        {
            //            cfg.Host(new Uri(Configuration["RabbitMQ:ConnectionString"]), h =>
            //                {
            //                    h.Username(Configuration["RabbitMQ:Username"]);
            //                    h.Password(Configuration["RabbitMQ:Password"]);
            //                });
            //        }
            //    });
            //});

            //services.AddMassTransitHostedService();

            services.AddSingleton<IWeatherService, WeatherService>();

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Foundant.Core.Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Foundant.Core.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
