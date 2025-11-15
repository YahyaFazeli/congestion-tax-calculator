using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class CongestionTaxDbContext(DbContextOptions<CongestionTaxDbContext> options) : DbContext(options)
{
    public DbSet<City> Cities => Set<City>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();
    public DbSet<TollInterval> TollIntervals => Set<TollInterval>();
    public DbSet<TollFreeDate> TollFreeDates => Set<TollFreeDate>();
    public DbSet<TollFreeMonth> TollFreeMonths => Set<TollFreeMonth>();
    public DbSet<TollFreeWeekday> TollFreeWeekdays => Set<TollFreeWeekday>();
    public DbSet<TollFreeVehicle> TollFreeVehicles => Set<TollFreeVehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CongestionTaxDbContext).Assembly);
    }
}
