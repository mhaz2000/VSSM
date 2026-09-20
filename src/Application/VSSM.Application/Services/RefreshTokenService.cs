using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VSSM.Application.Interfaces;
using VSSM.Application.Security;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services;

/// <summary>
/// [SECURITY - A07]: EF-backed refresh tokens. Raw values are high-entropy (64 random bytes,
/// Base64Url) and are only ever stored as a SHA-256 digest, so a DB leak does not expose usable
/// tokens. Rotation on every renewal makes a stolen token useless after the first use.
/// </summary>
public class RefreshTokenService(VSSMDbContext db, IOptions<RefreshTokenOptions> options, ILogger<RefreshTokenService> logger)
    : IRefreshTokenService
{
    // ---------------- ISSUE ----------------
    public async Task<string> IssueAsync(int userId, string ip, CancellationToken ct)
    {
        var token = Generate();
        await PurgeExpiredAsync(ct);

        db.TblRefreshTokens.Add(new TblRefreshToken
        {
            UserId = userId,
            TokenHash = Hash(token),
            CreatedUtc = DateTime.UtcNow,
            ExpiresUtc = DateTime.UtcNow.AddDays(options.Value.LifetimeDays),
            CreatedByIp = TruncateIp(ip)
        });
        await db.SaveChangesAsync(ct);
        return token;
    }

    // ---------------- VALIDATE ----------------
    public async Task<bool> TryValidateAsync(string rawToken, int userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return false;

        var row = await db.TblRefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == Hash(rawToken), ct);

        return row != null
            && row.UserId == userId
            && row.RevokedUtc == null
            && row.ExpiresUtc > DateTime.UtcNow;
    }

    // ---------------- ROTATE ----------------
    public async Task<string> RotateAsync(string rawToken, int userId, string ip, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            throw new InvalidOperationException("Invalid refresh token");

        var hash = Hash(rawToken);
        var row = await db.TblRefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (row == null || row.UserId != userId || row.RevokedUtc != null || row.ExpiresUtc <= DateTime.UtcNow)
            throw new InvalidOperationException("Invalid refresh token");

        var replacement = Generate();

        row.RevokedUtc = DateTime.UtcNow;
        row.ReplacedByTokenHash = Hash(replacement);

        db.TblRefreshTokens.Add(new TblRefreshToken
        {
            UserId = userId,
            TokenHash = Hash(replacement),
            CreatedUtc = DateTime.UtcNow,
            ExpiresUtc = DateTime.UtcNow.AddDays(options.Value.LifetimeDays),
            CreatedByIp = TruncateIp(ip),
            ReplacedTokenHash = hash
        });

        await db.SaveChangesAsync(ct);
        return replacement;
    }

    // ---------------- REVOKE ALL ----------------
    public async Task RevokeAllForUserAsync(int userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rows = await db.TblRefreshTokens
            .Where(t => t.UserId == userId && t.RevokedUtc == null && t.ExpiresUtc > now)
            .ToListAsync(ct);

        if (rows.Count == 0)
            return;

        foreach (var row in rows)
            row.RevokedUtc = now;

        await db.SaveChangesAsync(ct);
    }

    // ---------------- HELPERS ----------------
    private async Task PurgeExpiredAsync(CancellationToken ct)
    {
        var removed = await db.TblRefreshTokens
            .Where(t => t.ExpiresUtc <= DateTime.UtcNow && t.RevokedUtc != null)
            .ExecuteDeleteAsync(ct);

        if (removed > 0)
            logger.LogInformation("Purged {Count} expired refresh tokens", removed);
    }

    private static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string TruncateIp(string ip)
        => string.IsNullOrEmpty(ip) ? "unknown" : ip.Length > 45 ? ip[..45] : ip;
}