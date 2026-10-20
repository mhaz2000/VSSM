namespace VSSM.Application.Settings;

public interface ISecuritySettingsProvider
{
    SecuritySettings Current { get; }
    void Invalidate();
    Task ReloadAsync(CancellationToken ct = default);
}