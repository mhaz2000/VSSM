using VSSM.Application.Settings;

namespace VSSM.Presentation.Web.HostedServices;

public sealed class SecuritySettingsPreloadHostedService : BackgroundService
{
    private readonly ISecuritySettingsProvider _provider;
    public SecuritySettingsPreloadHostedService(ISecuritySettingsProvider provider) => _provider = provider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _provider.ReloadAsync(stoppingToken);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await _provider.ReloadAsync(stoppingToken);
        }
        catch (OperationCanceledException) { }
    }
}