using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TaxRuleRepository(CongestionTaxDbContext context) : Repository<TaxRule>(context), ITaxRuleRepository
{
    public async Task<TaxRule?> GetByCityAndYearAsync(
        Guid cityId,
        int year,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Include(r => r.Intervals)
            .Include(r => r.TollFreeDates)
            .Include(r => r.TollFreeMonths)
            .Include(r => r.TollFreeWeekdays)
            .Include(r => r.TollFreeVehicles)
            .FirstOrDefaultAsync(r => r.CityId == cityId && r.Year == year, cancellationToken);
    }

    public async Task<IEnumerable<TaxRule>> GetByCityIdAsync(
        Guid cityId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Include(r => r.Intervals)
            .Include(r => r.TollFreeDates)
            .Include(r => r.TollFreeMonths)
            .Include(r => r.TollFreeWeekdays)
            .Include(r => r.TollFreeVehicles)
            .Where(r => r.CityId == cityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaxRule?> GetByIdWithAllRelationsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Include(r => r.Intervals)
            .Include(r => r.TollFreeDates)
            .Include(r => r.TollFreeMonths)
            .Include(r => r.TollFreeWeekdays)
            .Include(r => r.TollFreeVehicles)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<TaxRule>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Include(r => r.Intervals)
            .Include(r => r.TollFreeDates)
            .Include(r => r.TollFreeMonths)
            .Include(r => r.TollFreeWeekdays)
            .Include(r => r.TollFreeVehicles)
            .ToListAsync(cancellationToken);
    }
}
