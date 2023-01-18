using Microsoft.Extensions.Caching.Memory;

namespace Application.Services.Cache;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheSetOptions;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _cacheSetOptions = new MemoryCacheEntryOptions
        {
            Size = 1024,
            SlidingExpiration = TimeSpan.FromMinutes(30),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(180)
        };

    }

    public Task<TItem?> GetItemAsync<TItem>(string key) =>
        Task.Run(() => _memoryCache.Get<TItem>(key));

    public Task SetItemAsync<TItem>(string key, TItem item) =>
        Task.Run(() => _memoryCache.Set(key, item, _cacheSetOptions));

    public Task RemoveItemAsync<TItem>(string key) =>
        Task.Run(() => _memoryCache.Remove(key));
}

