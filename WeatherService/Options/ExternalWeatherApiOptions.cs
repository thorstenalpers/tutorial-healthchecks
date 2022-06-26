namespace Examples.HealthCheck.WeatherService.Options
{
	using System;

	public class ExternalWeatherApiOptions
    {
        public const string KEY_NAME = "ExternalWeatherApiUri";

        public Uri Uri { get; set; }
    }
}