using Microsoft.Extensions.Caching.Memory;
using VSSM.Application.Settings;

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
    private readonly ISecuritySettingsProvider _settings;

    public RateLimitService(IMemoryCache cache, ISecuritySettingsProvider settings)
    {
        _cache = cache;
        _settings = settings;
    }

    public (bool blocked, int remainingSeconds) Check(string key)
    {
        if (_cache.TryGetValue<DateTimeOffset>("blk_" + key, out var end))
            return (true, Math.Max(0, (int)(end - DateTimeOffset.UtcNow).TotalSeconds));
        return (false, 0);
    }

    public void RegisterFailure(string key)
    {
        var s = _settings.Current;
        var window = TimeSpan.FromMinutes(s.WindowMinutes);

        var fkey = "fail_" + key;
        var cnt = _cache.GetOrCreate(fkey, e => { e.AbsoluteExpirationRelativeToNow = window; return 0; });
        cnt++;

        if (cnt >= s.Threshold)
        {
            _cache.Set("blk_" + key, DateTimeOffset.UtcNow.Add(window), window);
            _cache.Remove(fkey);
        }
        else _cache.Set(fkey, cnt, window);
    }

    public void Reset(string key)
    {
        _cache.Remove("fail_" + key);
        _cache.Remove("blk_" + key);
    }
}