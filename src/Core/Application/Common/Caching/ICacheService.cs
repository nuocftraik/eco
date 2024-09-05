

namespace ECO.WebApi.Application.Common.Caching;
public interface ICacheService
{
    // Lấy giá trị từ cache theo key. Nếu không tìm thấy, trả về null.
    T? Get<T>(string key);

    // Lấy giá trị từ cache theo key (phiên bản async). Nếu không tìm thấy, trả về null.
    Task<T?> GetAsync<T>(string key, CancellationToken token = default);

    // Làm mới mục cache, tức là gia hạn thời gian hết hạn của mục đó.
    void Refresh(string key);

    // Làm mới mục cache (phiên bản async).
    Task RefreshAsync(string key, CancellationToken token = default);

    // Xóa mục cache theo key.
    void Remove(string key);

    // Xóa mục cache theo key (phiên bản async).
    Task RemoveAsync(string key, CancellationToken token = default);

    // Lưu trữ giá trị vào cache với một thời gian hết hạn tùy chọn.
    void Set<T>(string key, T value, TimeSpan? slidingExpiration = null);

    // Lưu trữ giá trị vào cache (phiên bản async) với một thời gian hết hạn tùy chọn.
    Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
}
