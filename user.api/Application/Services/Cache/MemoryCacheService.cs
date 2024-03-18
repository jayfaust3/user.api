using Microsoft.Extensions.Caching.Memory;

namespace Application.Services.Cache;

public class MemoryCacheService : ICacheService
{
    private readonly double _slidingExpirationMinutes;
    private readonly double _absoluteExpirationRelativeToNowMinutes;
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _slidingExpirationMinutes = double.Parse
        (
            Environment.GetEnvironmentVariable("CACHE_SLIDING_EXPIRATION_MINUTES") ?? "30"
        );

        _absoluteExpirationRelativeToNowMinutes = double.Parse
        (
            Environment.GetEnvironmentVariable("ABSOLUTE_EXPIRATION_RELATIVE_TO_NOW_MINUTES") ?? "60"
        );

        _memoryCache = memoryCache;
    }

    public Task<TItem?> GetItemAsync<TItem>(string key) =>
        Task.Run(() => _memoryCache.Get<TItem>(key));

    public Task SetItemAsync<TItem>(string key, TItem item) =>
        Task.Run
        (
            () =>
                _memoryCache.Set
                (
                    key,
                    item,
                    new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(_slidingExpirationMinutes),
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_absoluteExpirationRelativeToNowMinutes)
                    }
                )
        );

    public Task RemoveItemAsync<TItem>(string key) =>
        Task.Run(() => _memoryCache.Remove(key));
}
