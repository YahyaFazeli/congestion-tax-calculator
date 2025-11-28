using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class CachedTaxRuleRepository : CachedRepository<TaxRule>, ITaxRuleRepository
{
    private readonly ITaxRuleRepository _innerRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedTaxRuleRepository> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public CachedTaxRuleRepository(
        ITaxRuleRepository innerRepository,
        IMemoryCache cache,
        ILogger<CachedTaxRuleRepository> logger
    )
        : base(innerRepository, cache, logger)
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TaxRule?> GetByCityAndYearAsync(
        Guid cityId,
        int year,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetCacheKey("ByCityAndYear", cityId, year);

        if (_cache.TryGetValue(key, out TaxRule? cached))
        {
            _logger.LogDebug("Cache hit for TaxRule. CityId: {CityId}, Year: {Year}", cityId, year);
            return cached;
        }

        _logger.LogDebug("Cache miss for TaxRule. CityId: {CityId}, Year: {Year}", cityId, year);
        var rule = await _innerRepository.GetByCityAndYearAsync(cityId, year, cancellationToken);

        if (rule != null)
        {
            _cache.Set(key, rule, _cacheExpiration);
        }

        return rule;
    }

    public async Task<IEnumerable<TaxRule>> GetByCityIdAsync(
        Guid cityId,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetCacheKey("ByCityId", cityId);

        if (_cache.TryGetValue(key, out IEnumerable<TaxRule>? cached))
        {
            _logger.LogDebug("Cache hit for TaxRules by CityId: {CityId}", cityId);
            return cached!;
        }

        _logger.LogDebug("Cache miss for TaxRules by CityId: {CityId}", cityId);
        var rules = await _innerRepository.GetByCityIdAsync(cityId, cancellationToken);

        _cache.Set(key, rules, _cacheExpiration);

        return rules;
    }

    public async Task<TaxRule?> GetByIdWithAllRelationsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetCacheKey("ByIdWithAllRelations", id);

        if (_cache.TryGetValue(key, out TaxRule? cached))
        {
            _logger.LogDebug("Cache hit for TaxRule with all relations. RuleId: {RuleId}", id);
            return cached;
        }

        _logger.LogDebug("Cache miss for TaxRule with all relations. RuleId: {RuleId}", id);
        var rule = await _innerRepository.GetByIdWithAllRelationsAsync(id, cancellationToken);

        if (rule != null)
        {
            _cache.Set(key, rule, _cacheExpiration);
        }

        return rule;
    }

    public async Task ReplaceRuleAsync(
        Guid oldRuleId,
        TaxRule newRule,
        CancellationToken cancellationToken = default
    )
    {
        await _innerRepository.ReplaceRuleAsync(oldRuleId, newRule, cancellationToken);
        InvalidateCache(oldRuleId);
        InvalidateCache(newRule.Id);
    }
}
