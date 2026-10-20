namespace VSSM.Application.Settings;

public sealed record SecuritySettings(int Threshold, int WindowMinutes)
{
    public const string ParentTitle = "تنظیمات امنیتی";
    public const string ThresholdTitle = "آستانه تلاش ناموفق";
    public const string WindowMinutesTitle = "پنجره زمانی (دقیقه)";

    public static readonly SecuritySettings Default = new(5, 15);

    public static bool IsSecurityTitle(string? title) =>
        title == ThresholdTitle || title == WindowMinutesTitle;

    public SecuritySettings Sanitized() => new(
        Threshold is > 0 and <= 1000 ? Threshold : Default.Threshold,
        WindowMinutes is > 0 and <= 1440 ? WindowMinutes : Default.WindowMinutes);
}