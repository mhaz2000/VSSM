using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VSSM.Application.Interfaces;

namespace VSSM.Application.Services.BackgroundServices;

/// <summary>
/// [AUDIT - FAU_STG]: enforces audit-log retention. Purges records older than
/// "Audit:RetentionDays" (appsettings, default 365) on startup and then once every 24h.
/// </summary>
public class AuditRetentionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuditRetentionBackgroundService> _logger;

    public AuditRetentionBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AuditRetentionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var days = 365;
                var retentionRaw = _configuration["Audit:RetentionDays"];
                if (!string.IsNullOrWhiteSpace(retentionRaw) && int.TryParse(retentionRaw, out var parsed))
                    days = parsed;
                var cutoff = DateTime.Now.AddDays(-days);

                using var scope = _scopeFactory.CreateScope();
                var audit = scope.ServiceProvider.GetRequiredService<IAuditService>();
                var removed = await audit.PurgeAsync(cutoff, stoppingToken);
                _logger.LogInformation("Audit retention: purged {Count} records older than {Cutoff:yyyy-MM-dd}", removed, cutoff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit retention purge failed");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
