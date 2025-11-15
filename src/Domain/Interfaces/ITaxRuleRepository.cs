using Domain.Entities;

namespace Domain.Interfaces;

public interface ITaxRuleRepository : IRepository<TaxRule>
{
    Task<TaxRule?> GetByCityAndYearAsync(
        Guid cityId,
        int year,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<TaxRule>> GetByCityIdAsync(
        Guid cityId,
        CancellationToken cancellationToken = default
    );

    Task<TaxRule?> GetByIdWithAllRelationsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
}
