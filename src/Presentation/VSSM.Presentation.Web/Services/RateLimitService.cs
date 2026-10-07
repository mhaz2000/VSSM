using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace VSSM.Presentation.Web.Services;

public interface IRateLimitService
{
    (bool blocked, int remainingSeconds) Check(string key);
    void RegisterFailure(string key);
    void Reset(string key);
}

public class RateLimitService : IRateLimitService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _window;
    private readonly int _threshold;

    public RateLimitService(IMemoryCache cache, IOptions<RateLimitOptions> options)
    {
        _cache = cache;
        _window = TimeSpan.FromMinutes(options.Value.WindowMinutes);
        _threshold = options.Value.Threshold;
    }

    public (bool blocked, int remainingSeconds) Check(string key)
    {
        if (_cache.TryGetValue<DateTimeOffset>("blk_" + key, out var end))
            return (true, Math.Max(0, (int)(end - DateTimeOffset.UtcNow).TotalSeconds));
        return (false, 0);
    }

    public void RegisterFailure(string key)
    {
        var fkey = "fail_" + key;
        var cnt = _cache.GetOrCreate(fkey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = _window;
            return 0;
        });
        cnt++;

        if (cnt >= _threshold)
        {
            _cache.Set("blk_" + key, DateTimeOffset.UtcNow.Add(_window), _window);
            _cache.Remove(fkey);
        }
        else
        {
            _cache.Set(fkey, cnt, _window);
        }
    }

    public void Reset(string key)
    {
        _cache.Remove("fail_" + key);
        _cache.Remove("blk_" + key);
    }
}