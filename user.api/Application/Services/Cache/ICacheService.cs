namespace Application.Services.Cache;

public interface ICacheService
{
    Task<TItem?> GetItemAsync<TItem>(string key);
    Task RemoveItemAsync<TItem>(string key);
    Task SetItemAsync<TItem>(string key, TItem item);
}