#nullable disable

using System;

namespace VSSM.Infrastructure.Data.Models;

/// <summary>
/// [SECURITY - A07]: persistent, revocable refresh token used to silently renew the cookie
/// session before the absolute 30-minute expiry. Only the SHA-256 digest (TokenHash) is stored —
/// the raw token lives solely in an HttpOnly cookie. Rotation keeps a chain via
/// ReplacedTokenHash / ReplacedByTokenHash so reuse after rotation can be detected.
/// </summary>
public partial class TblRefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime ExpiresUtc { get; set; }

    public DateTime? RevokedUtc { get; set; }

    public string CreatedByIp { get; set; }

    public string ReplacedTokenHash { get; set; }

    public string ReplacedByTokenHash { get; set; }

    public virtual TblUser User { get; set; }
}