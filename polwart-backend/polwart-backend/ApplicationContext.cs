using Microsoft.EntityFrameworkCore;
using polwart_backend.Entities;

namespace polwart_backend;

public sealed class ApplicationContext : DbContext
{
	public DbSet<Map> Maps => Set<Map>();
	
	public ApplicationContext() => Database.EnsureCreated();

	public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
	{
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"));
	}
}