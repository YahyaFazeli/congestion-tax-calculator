using Domain.Interfaces;
using Infrastructure.Interceptors;
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

        // Register repositories
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<ITaxRuleRepository, TaxRuleRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
