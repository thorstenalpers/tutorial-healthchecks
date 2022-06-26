namespace Examples.HealthCheck.WeatherService
{
	using Examples.HealthCheck.WeatherService.Models;
	using Microsoft.EntityFrameworkCore;

	public class MyDbContext : DbContext
	{
		public DbSet<WeatherForecast> WeatherForecasts { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options)
			=> options.UseSqlite("Data Source=DataAccess\\data.sqlite");
	}
}
