using Examples.HealthCheck.WeatherService.CustomHealthChecks;
using Examples.HealthCheck.WeatherService.Models;
using Examples.HealthCheck.WeatherService.Options;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Web;

namespace Examples.HealthCheck.WeatherService
{
	public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var hcOptions = new Options.HealthCheckOptions();
            Configuration.GetSection(Options.HealthCheckOptions.SECTION_NAME).Bind(hcOptions);

            services.Configure<Options.HealthCheckOptions>(Configuration.GetSection(Options.HealthCheckOptions.SECTION_NAME));
            services.Configure<ExternalWeatherApiOptions>(myOptions =>
            {
                myOptions.Uri = Configuration.GetValue<Uri>(ExternalWeatherApiOptions.KEY_NAME);
            });

			services.AddControllers();
            services.AddHttpClient();
			services.AddDbContext<MyDbContext>();

			services.AddMassTransit(x =>
			{
				x.UsingRabbitMq((context, cfg) =>
				{
					cfg.Host(host: "localhost", h =>
					{
						h.Username("admin");
						h.Password("password");
					});

					cfg.ConfigureEndpoints(context);
				});
			});

			services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherForecast Service", Version = "v1" });
            });

            var hcBuilder = services.AddHealthChecks();

			foreach(var uri in hcOptions.ExternalServiceUris)
			{
				hcBuilder.AddUrlGroup(uri, tags: new[] { nameof(EHealthCheckType.READINESS) });
			}
            
			hcBuilder.AddCheck<CustomDbCheck>(nameof(CustomDbCheck), tags: new[] { nameof(EHealthCheckType.READINESS) });
			//hcBuilder.AddCheck<CustomRabbitMqCheck>(nameof(CustomRabbitMqCheck), tags: new[] { nameof(EHealthCheckType.READINESS) });

			string rabbitConnectionString = $"amqp://admin:password@localhost";
			hcBuilder.AddRabbitMQ(rabbitConnectionString: rabbitConnectionString, name: "RabbitMqCheck", tags: new[] { nameof(EHealthCheckType.READINESS) });
			//hcBuilder.AddRabbitMQ(name: "RabbitMqCheck", tags: new[] { nameof(EHealthCheckType.READINESS) });


			hcBuilder.AddSqlite(Configuration["SqliteConnectionString"], name: "SqliteCheck", tags: new[] { nameof(EHealthCheckType.READINESS) });

			services.AddHealthChecksUI(setupSettings: setup =>
			{
				setup.AddHealthCheckEndpoint("Readiness", hcOptions.ApiPathReadiness);
				setup.AddHealthCheckEndpoint("Liveness", hcOptions.ApiPathLiveness);
			}).AddInMemoryStorage();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<Options.HealthCheckOptions> hcOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "sample_healthcheck v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks(hcOptions.Value.ApiPathReadiness, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    AllowCachingResponses = false,
                    Predicate = (check) => check.Tags.Contains(nameof(EHealthCheckType.READINESS)),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks(hcOptions.Value.ApiPathLiveness, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    AllowCachingResponses = false,
                    Predicate = (check) => check.Tags.Contains(nameof(EHealthCheckType.READINESS)) || check.Tags.Contains(nameof(EHealthCheckType.LIVENESS)),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
				endpoints.MapHealthChecksUI(setup =>
				{
					setup.UIPath = hcOptions.Value.UiPath;
				});
				endpoints.MapDefaultControllerRoute();
            });
        }
    }
}