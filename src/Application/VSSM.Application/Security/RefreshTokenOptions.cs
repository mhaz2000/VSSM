namespace VSSM.Application.Security;

/// <summary>
/// [SECURITY - A07]: configuration for the HttpOnly refresh-token session renewal.
/// Bound from the "Auth:RefreshToken" configuration section by
/// <see cref="ServiceCollectionExtensions"/>. Session length stays driven by the cookie
/// authentication middleware (ExpireTimeSpan = 30 min) so there is a single source of truth.
/// </summary>
public class RefreshTokenOptions
{
    public const string SectionName = "Auth:RefreshToken";

    /// <summary>
    /// Renew the cookie session when its remaining lifetime is at or below this
    /// many minutes (default 5). A higher value renews earlier but more often.
    /// </summary>
    public int RenewalWindowMinutes { get; set; } = 5;

    /// <summary>
    /// Lifetime of a refresh token in days (default 7). Once it expires the user must
    /// log in again with their credentials. Rotated on every renewal.
    /// </summary>
    public int LifetimeDays { get; set; } = 7;
}