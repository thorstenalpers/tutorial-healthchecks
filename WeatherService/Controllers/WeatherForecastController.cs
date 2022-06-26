using Examples.HealthCheck.WeatherService.Events;
using Examples.HealthCheck.WeatherService.Models;
using Examples.HealthCheck.WeatherService.Options;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Examples.HealthCheck.WeatherService.Controllers
{
	[ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ExternalWeatherApiOptions _externalWeatherApiOptions;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly MyDbContext _myDbContext;
		readonly IPublishEndpoint _publishEndpoint;

		public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IOptionsMonitor<ExternalWeatherApiOptions> externalWeatherApiOptionsMonitor,
            IHttpClientFactory httpClientFactory,
			MyDbContext myDbContext,
			IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _externalWeatherApiOptions = externalWeatherApiOptionsMonitor.CurrentValue;
			_httpClientFactory = httpClientFactory;
			_myDbContext = myDbContext;
			_publishEndpoint = publishEndpoint;
		}

		[HttpGet]
		public IEnumerable<WeatherForecast> GetFromDatabase()
		{
			return _myDbContext.WeatherForecasts.ToList();
		}

		[HttpGet]
		public async Task<ActionResult> PostToRabbitMqQueue()
		{
			string message = new String('X', 100000000);
			await _publishEndpoint.Publish<SomeEventReceived>(new
			{
				CorrelationId = Guid.NewGuid(),
				Message = message
			});
			return Ok();
		}

		[HttpGet]
        public async Task<IEnumerable<WeatherForecast>> GetFromExternalService()
        {
            var client = _httpClientFactory.CreateClient("weatherforecast");
            var response = await client.GetAsync(_externalWeatherApiOptions.Uri);
            var weatherForecasts = await response.Content.ReadAsAsync<IEnumerable<WeatherForecast>>();
            return weatherForecasts;
        }
    }
}