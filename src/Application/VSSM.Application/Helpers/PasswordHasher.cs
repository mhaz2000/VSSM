using System.Security.Cryptography;
using System.Text;

namespace VSSM.Application.Helpers;

/// <summary>
/// PBKDF2-based password hashing (HMAC-SHA256) using only the .NET BCL — no third-party dependency.
/// Stored format: "PBKDF2.{iterations}.{base64-salt}.{base64-hash}".
/// The marker prefix lets us detect legacy plaintext values for transparent upgrade on login.
/// </summary>
public static class PasswordHasher
{
    private const string Marker = "PBKDF2";
    private const int SaltBytes = 32;       // 256-bit salt
    private const int HashBytes = 32;       // 256-bit derived key
    private const int Iterations = 100_000; // OWASP-recommended ballpark for HMAC-SHA256

    /// <summary>
    /// Precomputed PBKDF2 hash used only to burn CPU time when a user does NOT exist,
    /// so the response time matches the "wrong password" path and prevents username
    /// enumeration via timing (OWASP A07).
    /// </summary>
    public static readonly string DummyHash = Hash("__timing_dummy__");

    /// <summary>Hashes a plaintext password into the self-describing storage format.</summary>
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithmName.SHA256, HashBytes);

        return $"{Marker}.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a plaintext password against a stored value. Returns false if the stored
    /// value is not in the hashed format — callers should fall back to legacy comparison
    /// and trigger a transparent upgrade.
    /// </summary>
    public static bool Verify(string password, string stored)
    {
        var parts = stored?.Split('.');
        if (parts is null || parts.Length != 4 || parts[0] != Marker)
            return false;

        if (!int.TryParse(parts[1], out var iterations))
            return false;

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        // Constant-time comparison to avoid timing-based oracle leaks.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    /// <summary>
    /// True when the stored value is a PBKDF2 hash (as opposed to a legacy plaintext password).
    /// Used by the login path to decide whether a transparent upgrade is needed.
    /// </summary>
    public static bool LooksHashed(string stored)
        => stored != null && stored.StartsWith(Marker + ".", StringComparison.Ordinal);

    /// <summary>
    /// [SECURITY - A07]: minimum password-strength rules (OWASP-aligned). Returns a list of
    /// human-readable (Persian) violations; empty means the password is acceptable. Enforced at
    /// user creation and on admin password change.
    /// </summary>
    public static List<string> ValidateStrength(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password) || password.Length < 8)
            errors.Add("رمز عبور باید حداقل ۸ کاراکتر باشد.");

        if (password.Length > 64)
            errors.Add("رمز عبور نباید بیش از ۶۴ کاراکتر باشد.");

        if (!password.Any(char.IsUpper))
            errors.Add("رمز عبور باید شامل حداقل یک حرف بزرگ باشد.");

        if (!password.Any(char.IsLower))
            errors.Add("رمز عبور باید شامل حداقل یک حرف کوچک باشد.");

        if (!password.Any(char.IsDigit))
            errors.Add("رمز عبور باید شامل حداقل یک عدد باشد.");

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            errors.Add("رمز عبور باید شامل حداقل یک نویسه ویژه (مانند !@#$%) باشد.");

        return errors;
    }
}
