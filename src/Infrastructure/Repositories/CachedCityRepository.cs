using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class CachedCityRepository : CachedRepository<City>, ICityRepository
{
    private readonly ICityRepository _innerRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedCityRepository> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public CachedCityRepository(
        ICityRepository innerRepository,
        IMemoryCache cache,
        ILogger<CachedCityRepository> logger
    )
        : base(innerRepository, cache, logger)
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<City?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetCacheKey("ByName", name);

        if (_cache.TryGetValue(key, out City? cached))
        {
            _logger.LogDebug("Cache hit for City by name: {Name}", name);
            return cached;
        }

        _logger.LogDebug("Cache miss for City by name: {Name}", name);
        var city = await _innerRepository.GetByNameAsync(name, cancellationToken);

        if (city != null)
        {
            _cache.Set(key, city, _cacheExpiration);
        }

        return city;
    }

    public async Task<City?> GetByIdWithRulesAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetCacheKey("ByIdWithRules", id);

        if (_cache.TryGetValue(key, out City? cached))
        {
            _logger.LogDebug("Cache hit for City with rules. CityId: {CityId}", id);
            return cached;
        }

        _logger.LogDebug("Cache miss for City with rules. CityId: {CityId}", id);
        var city = await _innerRepository.GetByIdWithRulesAsync(id, cancellationToken);

        if (city != null)
        {
            _cache.Set(key, city, _cacheExpiration);
        }

        return city;
    }

    public async Task<City?> GetByIdWithDetailedRulesAsync(
        Guid id,
        Guid ruleId,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetCacheKey("ByIdWithDetailedRules", id, ruleId);

        if (_cache.TryGetValue(key, out City? cached))
        {
            _logger.LogDebug(
                "Cache hit for City with detailed rules. CityId: {CityId}, RuleId: {RuleId}",
                id,
                ruleId
            );
            return cached;
        }

        _logger.LogDebug(
            "Cache miss for City with detailed rules. CityId: {CityId}, RuleId: {RuleId}",
            id,
            ruleId
        );
        var city = await _innerRepository.GetByIdWithDetailedRulesAsync(
            id,
            ruleId,
            cancellationToken
        );

        if (city != null)
        {
            _cache.Set(key, city, _cacheExpiration);
        }

        return city;
    }
}
