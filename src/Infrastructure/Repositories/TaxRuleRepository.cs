using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class TaxRuleRepository(CongestionTaxDbContext context, ILogger<TaxRuleRepository> logger)
    : Repository<TaxRule>(context),
        ITaxRuleRepository
{
    public async Task<TaxRule?> GetByCityAndYearAsync(
        Guid cityId,
        int year,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Fetching tax rule for CityId: {CityId}, Year: {Year}", cityId, year);

        var result = await _dbSet
            .Include(r => r.Intervals)
            .Include(r => r.TollFreeDates)
            .Include(r => r.TollFreeMonths)
            .Include(r => r.TollFreeWeekdays)
            .Include(r => r.TollFreeVehicles)
            .FirstOrDefaultAsync(r => r.CityId == cityId && r.Year == year, cancellationToken);

        logger.LogDebug("Tax rule query completed. Found: {Found}", result != null);

        return result;
    }

    public async Task<IEnumerable<TaxRule>> GetByCityIdAsync(
        Guid cityId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Fetching all tax rules for CityId: {CityId}", cityId);

        var result = await _dbSet
            .Include(r => r.Intervals)
            .Include(r => r.TollFreeDates)
            .Include(r => r.TollFreeMonths)
            .Include(r => r.TollFreeWeekdays)
            .Include(r => r.TollFreeVehicles)
            .Where(r => r.CityId == cityId)
            .ToListAsync(cancellationToken);

        logger.LogDebug("Tax rules query completed. Count: {Count}", result.Count);

        return result;
    }

    public async Task<TaxRule?> GetByIdWithAllRelationsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Fetching tax rule with all relations. RuleId: {RuleId}", id);

        var result = await _dbSet
            .Include(r => r.Intervals)
            .Include(r => r.TollFreeDates)
            .Include(r => r.TollFreeMonths)
            .Include(r => r.TollFreeWeekdays)
            .Include(r => r.TollFreeVehicles)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        logger.LogDebug("Tax rule query completed. Found: {Found}", result != null);

        return result;
    }

    public override async Task<IEnumerable<TaxRule>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Fetching all tax rules");

        var result = await _dbSet
            .Include(r => r.Intervals)
            .Include(r => r.TollFreeDates)
            .Include(r => r.TollFreeMonths)
            .Include(r => r.TollFreeWeekdays)
            .Include(r => r.TollFreeVehicles)
            .ToListAsync(cancellationToken);

        logger.LogDebug("All tax rules query completed. Count: {Count}", result.Count);

        return result;
    }

    public async Task ReplaceRuleAsync(
        Guid oldRuleId,
        TaxRule newRule,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Replacing tax rule. OldRuleId: {OldRuleId}, NewRuleId: {NewRuleId}", oldRuleId, newRule.Id);

        try
        {
            var oldRule = await GetByIdWithAllRelationsAsync(oldRuleId, cancellationToken);

            if (oldRule is not null)
            {
                _dbSet.Remove(oldRule);
            }

            await _dbSet.AddAsync(newRule, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            logger.LogDebug("Replace rule operation completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error replacing tax rule. OldRuleId: {OldRuleId}", oldRuleId);
            throw;
        }
    }
}
