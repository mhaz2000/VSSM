namespace VSSM.Presentation.Web.Services;

public class RateLimitOptions
{
    public const string SectionName = "Security:RateLimit";

    public int Threshold { get; set; } = 5;
    public int WindowMinutes { get; set; } = 15;
}