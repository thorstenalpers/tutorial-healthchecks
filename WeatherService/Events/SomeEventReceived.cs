using System;

namespace Examples.HealthCheck.WeatherService.Events
{
	public interface SomeEventReceived
	{
		public Guid CorrelationId { get; set; }
		public string Message { get; set; }
	}
}
