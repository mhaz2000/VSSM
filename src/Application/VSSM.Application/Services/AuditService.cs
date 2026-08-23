using System;
using System.Threading;
using System.Threading.Tasks;
using VSSM.Application.Interfaces;
using VSSM.Infrastructure.Data;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services;

/// <summary>
/// [AUDIT - FAU_GEN/SAR/STG]: persists audit records to the AppLog table.
/// Only non-sensitive metadata is stored (actor, source IP, event, outcome) — never passwords or raw request bodies.
/// </summary>
public class AuditService : IAuditService
{
    private readonly VSSMDbContext _db;

    public AuditService(VSSMDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        string level,
        string message,
        string? userName = null,
        string? ipAddress = null,
        string? path = null,
        string? method = null,
        int? statusCode = null,
        string? exception = null,
        CancellationToken cancellationToken = default)
    {
        _db.AppLogs.Add(new AppLog
        {
            Level = level,
            Message = message,
            UserName = userName,
            IpAddress = ipAddress,
            Path = path,
            Method = method,
            StatusCode = statusCode,
            Exception = exception,
            TimeStamp = DateTime.Now
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
