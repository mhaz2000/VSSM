using System.Threading;
using System.Threading.Tasks;

namespace VSSM.Application.Interfaces;

/// <summary>
/// [SECURITY - A07]: persistent refresh tokens that silently renew the cookie session before
/// its absolute expiry. Only SHA-256 digests are stored; the raw token lives in an HttpOnly cookie.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Creates a new refresh token for the user and returns the raw value.</summary>
    Task<string> IssueAsync(int userId, string ip, CancellationToken ct);

    /// <summary>
    /// True when the token exists, belongs to <paramref name="userId"/>, is not revoked and has
    /// not expired. Used by the renewal middleware before extending the session.
    /// </summary>
    Task<bool> TryValidateAsync(string rawToken, int userId, CancellationToken ct);

    /// <summary>
    /// Revokes the given token and issues a replacement with a fresh lifetime so each renewal
    /// produces a brand-new secret (rotation). Throws <see cref="System.InvalidOperationException"/>
    /// when the token is missing, foreign, already revoked or expired.
    /// </summary>
    Task<string> RotateAsync(string rawToken, int userId, string ip, CancellationToken ct);

    /// <summary>Revokes every currently valid token for a user (logout / password change / deactivation).</summary>
    Task RevokeAllForUserAsync(int userId, CancellationToken ct);
}