using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

    public async Task<IEnumerable<AppLog>> QueryAsync(
        string? userName = null,
        string? level = null,
        string? message = null,
        DateTime? from = null,
        DateTime? to = null,
        int max = 500,
        CancellationToken cancellationToken = default)
    {
        var query = _db.AppLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(x => x.UserName != null && x.UserName.Contains(userName));

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(x => x.Level == level);

        if (!string.IsNullOrWhiteSpace(message))
            query = query.Where(x => x.Message != null && x.Message.Contains(message));

        if (from.HasValue)
            query = query.Where(x => x.TimeStamp >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.TimeStamp <= to.Value);

        return await query
            .OrderByDescending(x => x.TimeStamp)
            .Take(max)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> PurgeAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        var old = await _db.AppLogs
            .Where(x => x.TimeStamp < olderThan)
            .ToListAsync(cancellationToken);

        _db.AppLogs.RemoveRange(old);
        await _db.SaveChangesAsync(cancellationToken);
        return old.Count;
    }
}
