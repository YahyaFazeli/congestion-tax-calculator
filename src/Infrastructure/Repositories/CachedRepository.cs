using Domain.Base;
using Domain.Common;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class CachedRepository<T> : IRepository<T>
    where T : Entity
{
    private readonly IRepository<T> _innerRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedRepository<T>> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public CachedRepository(
        IRepository<T> innerRepository,
        IMemoryCache cache,
        ILogger<CachedRepository<T>> logger
    )
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetCacheKey("ById", id);

        if (_cache.TryGetValue(key, out T? cached))
        {
            _logger.LogDebug("Cache hit for {EntityType} with Id: {Id}", typeof(T).Name, id);
            return cached;
        }

        _logger.LogDebug("Cache miss for {EntityType} with Id: {Id}", typeof(T).Name, id);
        var entity = await _innerRepository.GetByIdAsync(id, cancellationToken);

        if (entity != null)
        {
            _cache.Set(key, entity, _cacheExpiration);
        }

        return entity;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        var key = GetCacheKey("All");

        if (_cache.TryGetValue(key, out IEnumerable<T>? cached))
        {
            _logger.LogDebug("Cache hit for all {EntityType}", typeof(T).Name);
            return cached!;
        }

        _logger.LogDebug("Cache miss for all {EntityType}", typeof(T).Name);
        var entities = await _innerRepository.GetAllAsync(cancellationToken);

        _cache.Set(key, entities, _cacheExpiration);

        return entities;
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var key = GetCacheKey("Paged", page, pageSize);

        if (_cache.TryGetValue(key, out PagedResult<T>? cached))
        {
            _logger.LogDebug(
                "Cache hit for paged {EntityType} (Page: {Page}, Size: {PageSize})",
                typeof(T).Name,
                page,
                pageSize
            );
            return cached!;
        }

        _logger.LogDebug(
            "Cache miss for paged {EntityType} (Page: {Page}, Size: {PageSize})",
            typeof(T).Name,
            page,
            pageSize
        );
        var result = await _innerRepository.GetPagedAsync(page, pageSize, cancellationToken);

        _cache.Set(key, result, _cacheExpiration);

        return result;
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var result = await _innerRepository.AddAsync(entity, cancellationToken);
        InvalidateCache();
        return result;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _innerRepository.UpdateAsync(entity, cancellationToken);
        InvalidateCache(entity.Id);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _innerRepository.DeleteAsync(id, cancellationToken);
        InvalidateCache(id);
    }

    public virtual async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _innerRepository.ExistsAsync(id, cancellationToken);
    }

    protected void InvalidateCache(Guid? id = null)
    {
        _logger.LogDebug("Invalidating cache for {EntityType}", typeof(T).Name);

        // Remove specific entity cache if id provided
        if (id.HasValue)
        {
            _cache.Remove(GetCacheKey("ById", id.Value));
        }

        // Remove collection caches
        _cache.Remove(GetCacheKey("All"));

        // Note: Paged results will expire naturally or could be tracked separately
    }

    protected string GetCacheKey(string operation, params object[] parameters)
    {
        var paramString = parameters.Length > 0 ? $"_{string.Join("_", parameters)}" : "";
        return $"{typeof(T).Name}_{operation}{paramString}";
    }
}
