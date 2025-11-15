using Domain.Entities;

namespace Domain.Interfaces;

public interface ICityRepository : IRepository<City>
{
    Task<City?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<City?> GetByIdWithRulesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<City?> GetByIdWithDetailedRulesAsync(
        Guid id,
        Guid ruleId,
        CancellationToken cancellationToken = default
    );
}
