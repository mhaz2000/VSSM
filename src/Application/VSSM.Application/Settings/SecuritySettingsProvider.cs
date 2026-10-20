using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSSM.Application.Settings;
using VSSM.Infrastructure.Data;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Infrastructure.Settings;

public sealed class SecuritySettingsProvider(
    IServiceScopeFactory scopeFactory,
    ILogger<SecuritySettingsProvider> logger) : ISecuritySettingsProvider
{
    private volatile SecuritySettings _current = SecuritySettings.Default;
    private int _reloadInProgress;

    public SecuritySettings Current => _current;

    public void Invalidate() => _ = ReloadAsync();

    public async Task ReloadAsync(CancellationToken ct = default)
    {
        if (Interlocked.Exchange(ref _reloadInProgress, 1) == 1) return;

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<VSSMDbContext>();

            var rows = await db.TblBaseInfo.AsNoTracking()
                .Where(x => x.Title == SecuritySettings.ThresholdTitle
                         || x.Title == SecuritySettings.WindowMinutesTitle)
                .Select(x => new { x.Title, x.Code })
                .ToListAsync(ct);

            int threshold = SecuritySettings.Default.Threshold;
            int window = SecuritySettings.Default.WindowMinutes;

            foreach (var row in rows)
            {
                if (row.Title == SecuritySettings.ThresholdTitle && int.TryParse(row.Code, out var t))
                    threshold = t;
                else if (row.Title == SecuritySettings.WindowMinutesTitle && int.TryParse(row.Code, out var w))
                    window = w;
            }

            _current = new SecuritySettings(threshold, window).Sanitized();

            logger.LogInformation(
                "SecuritySettings reloaded: Threshold={Threshold}, WindowMinutes={WindowMinutes}",
                _current.Threshold, _current.WindowMinutes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Reload failed, keeping previous Threshold={Threshold}, WindowMinutes={WindowMinutes}",
                _current.Threshold, _current.WindowMinutes);
        }
        finally
        {
            Interlocked.Exchange(ref _reloadInProgress, 0);
        }
    }
}