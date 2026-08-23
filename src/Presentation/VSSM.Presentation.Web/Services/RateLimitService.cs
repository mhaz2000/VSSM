using Microsoft.Extensions.Caching.Memory;

namespace VSSM.Presentation.Web.Services;

public interface IRateLimitService
{
    /// <summary>Returns whether the key is currently blocked and seconds remaining.</summary>
    (bool blocked, int remainingSeconds) Check(string key);

    /// <summary>Records a failed attempt; blocks the key once the threshold is reached.</summary>
    void RegisterFailure(string key);

    /// <summary>Clears failure/block state for a key (call on success).</summary>
    void Reset(string key);
}

/// <summary>
/// In-memory brute-force protection for login. Uses IMemoryCache only
/// (no distributed cache). Blocks an IP / username after N consecutive failures
/// for a cool-down window.
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    private const int Threshold = 5;

    public RateLimitService(IMemoryCache cache) => _cache = cache;

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
            e.AbsoluteExpirationRelativeToNow = Window;
            return 0;
        });
        cnt++;

        if (cnt >= Threshold)
        {
            _cache.Set("blk_" + key, DateTimeOffset.UtcNow.Add(Window), Window);
            _cache.Remove(fkey);
        }
        else
        {
            _cache.Set(fkey, cnt, Window);
        }
    }

    public void Reset(string key)
    {
        _cache.Remove("fail_" + key);
        _cache.Remove("blk_" + key);
    }
}
