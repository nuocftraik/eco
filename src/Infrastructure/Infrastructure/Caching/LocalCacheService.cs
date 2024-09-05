

using ECO.WebApi.Application.Common.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Caching;
public class LocalCacheService : ICacheService
{
    private readonly ILogger<LocalCacheService> _logger;
    private readonly IMemoryCache _cache;

    public LocalCacheService(ILogger<LocalCacheService> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }
    public T? Get<T>(string key) =>
        _cache.Get<T>(key);

    public Task<T?> GetAsync<T>(string key, CancellationToken token = default) =>
        Task.FromResult(Get<T>(key)); // Vì việc lấy dữ liệu từ cache là đồng bộ, chúng ta chỉ cần gói trong Task.

    // Làm mới mục cache, gia hạn thời gian hết hạn cho mục đó
    public void Refresh(string key) =>
        _cache.TryGetValue(key, out _); // Kiểm tra sự tồn tại của key, nếu có thì gia hạn thời gian hết hạn

    // Làm mới mục cache (phiên bản async).
    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        Refresh(key); // Gọi lại phương thức Refresh đồng bộ
        return Task.CompletedTask; // Trả về Task hoàn thành
    }

    // Xóa mục cache theo key.
    public void Remove(string key) =>
        _cache.Remove(key); 

    // Xóa mục cache (phiên bản async).
    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key); // Gọi phương thức Remove đồng bộ
        return Task.CompletedTask; // Trả về Task hoàn thành
    }

    // Lưu trữ giá trị vào cache với thời gian hết hạn tùy chọn
    public void Set<T>(string key, T value, TimeSpan? slidingExpiration = null)
    {
        // Nếu không có giá trị thời gian hết hạn nào được chỉ định, mặc định là 10 phút
        slidingExpiration ??= TimeSpan.FromMinutes(10);

        // Đặt giá trị vào cache và cấu hình thời gian hết hạn (sliding expiration)
        _cache.Set(key, value, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });

        // Ghi log thông tin về việc thêm dữ liệu vào cache
        _logger.LogDebug($"Added to Cache : {key}");
    }

    // Lưu trữ giá trị vào cache (phiên bản async)
    public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken token = default)
    {
        Set(key, value, slidingExpiration); // Gọi lại phương thức Set đồng bộ
        return Task.CompletedTask; // Trả về Task hoàn thành
    }
}
