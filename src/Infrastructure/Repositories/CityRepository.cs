using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class CityRepository(CongestionTaxDbContext context, ILogger<CityRepository> logger)
    : Repository<City>(context),
        ICityRepository
{
    public override async Task<IEnumerable<City>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Fetching all cities");

        var result = await _dbSet.Include(c => c.TaxRules).ToListAsync(cancellationToken);

        logger.LogDebug("All cities query completed. Count: {Count}", result.Count);

        return result;
    }

    public async Task<City?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Fetching city by name: {CityName}", name);

        var result = await _dbSet.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

        logger.LogDebug("City by name query completed. Found: {Found}", result != null);

        return result;
    }

    public async Task<City?> GetByIdWithRulesAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Fetching city with rules. CityId: {CityId}", id);

        var result = await _dbSet
            .Include(c => c.TaxRules)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        logger.LogDebug("City with rules query completed. Found: {Found}", result != null);

        return result;
    }

    public async Task<City?> GetByIdWithDetailedRulesAsync(
        Guid id,
        Guid ruleId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug(
            "Fetching city with detailed rules. CityId: {CityId}, RuleId: {RuleId}",
            id,
            ruleId
        );

        var result = await _dbSet
            .Include(c => c.TaxRules)
                .ThenInclude(r => r.Intervals)
            .Include(c => c.TaxRules)
                .ThenInclude(r => r.TollFreeDates)
            .Include(c => c.TaxRules)
                .ThenInclude(r => r.TollFreeMonths)
            .Include(c => c.TaxRules)
                .ThenInclude(r => r.TollFreeWeekdays)
            .Include(c => c.TaxRules)
                .ThenInclude(r => r.TollFreeVehicles)
            .Where(c => c.Id == id && c.TaxRules.Any(d => d.Id == ruleId))
            .FirstOrDefaultAsync(cancellationToken);

        logger.LogDebug("City with detailed rules query completed. Found: {Found}", result != null);

        return result;
    }
}
