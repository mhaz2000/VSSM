using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSSM.Infrastructure.Data.Models;

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

    /// [FAU_SAR]: retrieve audit records for review (filtered).
    Task<IEnumerable<AppLog>> QueryAsync(
        string? userName = null,
        string? level = null,
        string? message = null,
        DateTime? from = null,
        DateTime? to = null,
        int max = 500,
        CancellationToken cancellationToken = default);

    /// [FAU_STG]: delete records older than the given timestamp (retention).
    Task<int> PurgeAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
