using Domain.Interfaces;
using Infrastructure.Interceptors;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Register the slow query interceptor
        services.AddSingleton<SlowQueryInterceptor>();

        // Add DbContext with PostgreSQL
        services.AddDbContext<CongestionTaxDbContext>(
            (serviceProvider, options) =>
            {
                var connectionString =
                    configuration.GetConnectionString("CongestionTaxDb")
                    ?? throw new InvalidOperationException(
                        "Connection string 'CongestionTaxDb' not found."
                    );

                var interceptor = serviceProvider.GetRequiredService<SlowQueryInterceptor>();

                options
                    .UseNpgsql(
                        connectionString,
                        npgsqlOptions =>
                        {
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorCodesToAdd: null
                            );
                        }
                    )
                    .AddInterceptors(interceptor);
            }
        );

        // Register memory cache
        services.AddMemoryCache();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register base repositories (non-cached)
        services.AddScoped<CityRepository>();
        services.AddScoped<TaxRuleRepository>();

        // Register cached repositories
        services.AddScoped<ICityRepository>(sp =>
        {
            var innerRepo = sp.GetRequiredService<CityRepository>();
            var cache = sp.GetRequiredService<IMemoryCache>();
            var logger = sp.GetRequiredService<ILogger<CachedCityRepository>>();
            return new CachedCityRepository(innerRepo, cache, logger);
        });

        services.AddScoped<ITaxRuleRepository>(sp =>
        {
            var innerRepo = sp.GetRequiredService<TaxRuleRepository>();
            var cache = sp.GetRequiredService<IMemoryCache>();
            var logger = sp.GetRequiredService<ILogger<CachedTaxRuleRepository>>();
            return new CachedTaxRuleRepository(innerRepo, cache, logger);
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
