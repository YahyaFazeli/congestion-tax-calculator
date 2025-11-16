using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CityRepository(CongestionTaxDbContext context)
    : Repository<City>(context),
        ICityRepository
{
    public override async Task<IEnumerable<City>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.Include(c => c.TaxRules).ToListAsync(cancellationToken);
    }

    public async Task<City?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<City?> GetByIdWithRulesAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Include(c => c.TaxRules)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<City?> GetByIdWithDetailedRulesAsync(
        Guid id,
        Guid ruleId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
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
    }

    public async Task<TaxRule> AddTaxRuleAsync(
        TaxRule taxRule,
        CancellationToken cancellationToken = default
    )
    {
        await _context.TaxRules.AddAsync(taxRule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return taxRule;
    }
}
