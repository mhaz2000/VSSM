using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace VSSM.Presentation.Web.Services;

public class CaptchaResult
{
    public string Token { get; set; } = string.Empty;
    public string Svg { get; set; } = string.Empty;
}

public interface ICaptchaService
{
    CaptchaResult Generate();
    bool Validate(string token, string input);
}

/// <summary>
/// Self-hosted "messy characters" CAPTCHA (no third-party / Google service).
/// The expected code is stored server-side in IMemoryCache keyed by a one-time token.
/// Rendered as inline SVG (no System.Drawing dependency, cross-platform safe).
/// </summary>
public class CaptchaService : ICaptchaService
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan Life = TimeSpan.FromMinutes(5);
    private const int Length = 5;
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static readonly Random Rnd = new();

    public CaptchaService(IMemoryCache cache) => _cache = cache;

    public CaptchaResult Generate()
    {
        var code = new StringBuilder(Length);
        for (int i = 0; i < Length; i++)
            code.Append(Chars[RandomNumberGenerator.GetInt32(Chars.Length)]);

        var token = Guid.NewGuid().ToString("N");
        _cache.Set("cap_" + token, code.ToString(), Life);
        return new CaptchaResult { Token = token, Svg = Render(code.ToString()) };
    }

    public bool Validate(string token, string input)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(input))
            return false;

        if (_cache.TryGetValue<string>("cap_" + token, out var code))
        {
            _cache.Remove("cap_" + token); // one-time use
            return string.Equals(code, input.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static string Render(string code)
    {
        const int w = 190, h = 56;
        var sb = new StringBuilder();
        sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{w}' height='{h}' viewBox='0 0 {w} {h}'>");
        sb.Append("<rect width='100%' height='100%' fill='#eef2f7'/>");

        for (int i = 0; i < 7; i++)
        {
            var x1 = Rnd.Next(w); var y1 = Rnd.Next(h);
            var x2 = Rnd.Next(w); var y2 = Rnd.Next(h);
            sb.Append($"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='{RandColor()}' stroke-width='{Rnd.Next(1, 2)}' opacity='0.6'/>");
        }

        for (int i = 0; i < 45; i++)
            sb.Append($"<circle cx='{Rnd.Next(w)}' cy='{Rnd.Next(h)}' r='{Rnd.Next(1, 2)}' fill='{RandColor()}' opacity='0.5'/>");

        int x = 18;
        foreach (var c in code)
        {
            var fontSize = 26 + Rnd.Next(0, 12);
            var y = 36 + Rnd.Next(-7, 7);
            var rot = Rnd.Next(-28, 28);
            var font = Rnd.Next(2) == 0 ? "'Courier New',monospace" : "'Comic Sans MS',cursive";
            sb.Append($"<text x='{x}' y='{y}' font-size='{fontSize}' font-family={font} font-weight='bold' " +
                      $"fill='{RandColor()}' transform='rotate({rot} {x} {y})'>{c}</text>");
            x += 32;
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    private static string RandColor()
    {
        var hue = Rnd.Next(0, 360);
        var sat = Rnd.Next(45, 75);
        var light = Rnd.Next(25, 45);
        return $"hsl({hue},{sat}%,{light}%)";
    }
}
