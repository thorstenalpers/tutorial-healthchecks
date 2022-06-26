using MassTransit;
using MassTransit.Monitoring.Health;
using MassTransit.Registration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Examples.HealthCheck.WeatherService.CustomHealthChecks
{
	public class CustomRabbitMqCheck : IHealthCheck
	{
		private readonly IConnection _connection;

		public CustomRabbitMqCheck(IHttpClientFactory httpClientFactory, IConnection connection)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			Stopwatch stopWatch = new Stopwatch();
			var isHealthy = _connection.IsOpen;
			stopWatch.Stop();
			Trace.WriteLine($"RabbitMq Check Elapsed={stopWatch.ElapsedMilliseconds}ms");
			return isHealthy ? HealthCheckResult.Healthy("Healthy") : HealthCheckResult.Unhealthy("Unhealthy", null);
		}
	}
}