using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Add DbContext with PostgreSQL
        services.AddDbContext<CongestionTaxDbContext>(options =>
        {
            var connectionString =
                configuration.GetConnectionString("CongestionTaxDb")
                ?? throw new InvalidOperationException(
                    "Connection string 'CongestionTaxDb' not found."
                );

            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null
                    );
                }
            );
        });

        // Register repositories
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<ITaxRuleRepository, TaxRuleRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
