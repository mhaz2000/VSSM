using System.Threading;
using System.Threading.Tasks;

namespace VSSM.Application.Interfaces;

/// <summary>
/// [AUDIT - FAU_GEN/SAR/STG]: emits security-relevant audit records.
/// The sink is intentionally unspecified here — callers log the *what* (event, actor, source,
/// outcome); storage is delegated to the implementation (currently the AppLog table).
/// </summary>
public interface IAuditService
{
    Task LogAsync(
        string level,
        string message,
        string? userName = null,
        string? ipAddress = null,
        string? path = null,
        string? method = null,
        int? statusCode = null,
        string? exception = null,
        CancellationToken cancellationToken = default);
}
