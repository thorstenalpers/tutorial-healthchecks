using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Examples.HealthCheck.WeatherService.CustomHealthChecks
{
	public class CustomDbCheck : IHealthCheck
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly MyDbContext _myDbContext;

		public CustomDbCheck(IHttpClientFactory httpClientFactory, MyDbContext myDbContext)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_myDbContext = myDbContext ?? throw new ArgumentNullException(nameof(myDbContext));
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			bool isHealthy = _myDbContext.WeatherForecasts.Any();
			stopwatch.Stop();
			Debug.WriteLine($"Elapsed Time of CustomDbCheck= {stopwatch.ElapsedMilliseconds}ms");
			return isHealthy ? HealthCheckResult.Healthy("Healthy") : HealthCheckResult.Unhealthy("Unhealthy", null);
		}
	}
}