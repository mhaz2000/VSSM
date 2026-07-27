using System.Globalization;
using System.Text.RegularExpressions;

namespace VSSM.Application.Helpers;

public static class DateTimeHelper
{
    public static string ToShamsi(this DateTime date)
    {
        PersianCalendar pc = new PersianCalendar();

        int year = pc.GetYear(date);
        int month = pc.GetMonth(date);
        int day = pc.GetDayOfMonth(date);

        var shamsiDate = $"{year:0000}/{month:00}/{day:00}";

        return shamsiDate;
    }

    public static string ToShamsi(this DateOnly date)
    {
        var d = new DateTime(date, new TimeOnly());
        PersianCalendar pc = new PersianCalendar();

        int year = pc.GetYear(d);
        int month = pc.GetMonth(d);
        int day = pc.GetDayOfMonth(d);

        var shamsiDate = $"{year:0000}/{month:00}/{day:00}";

        return shamsiDate;
    }

    public static DateTime ToGregorian(string persianDate)
    {
        if (string.IsNullOrWhiteSpace(persianDate))
            throw new ArgumentException("تاریخ معتبر نیست");

        persianDate = persianDate
            .Replace("۰", "0")
            .Replace("۱", "1")
            .Replace("۲", "2")
            .Replace("۳", "3")
            .Replace("۴", "4")
            .Replace("۵", "5")
            .Replace("۶", "6")
            .Replace("۷", "7")
            .Replace("۸", "8")
            .Replace("۹", "9");

        var parts = persianDate.Split('/');
        if (parts.Length != 3)
            throw new FormatException("فرمت تاریخ صحیح نیست");

        int year = int.Parse(parts[0]);
        int month = int.Parse(parts[1]);
        int day = int.Parse(parts[2]);

        var pc = new PersianCalendar();
        return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
    }

    public static DateTime? ToMiladi(this string date)
    {
        DateTime? miladiDate = null;

        if (!string.IsNullOrWhiteSpace(date))
        {
            date = DigitHelper.ToEnglishNumber(date);

            var parts = date.Split('/');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            PersianCalendar pc = new PersianCalendar();
            miladiDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

        return miladiDate;
    }

    public static DateTime ParseWhetherGeorgianOrJalli(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentNullException(nameof(input));

        input = input.Trim();

        // Accept / or -
        var match = Regex.Match(input, @"^(?<y>\d{4})[-/](?<m>\d{1,2})[-/](?<d>\d{1,2})$");

        if (!match.Success)
            throw new FormatException("Invalid date format.");

        int year = int.Parse(match.Groups["y"].Value);
        int month = int.Parse(match.Groups["m"].Value);
        int day = int.Parse(match.Groups["d"].Value);

        // Persian (Shamsi)
        if (year < 1700)
        {
            var pc = new PersianCalendar();
            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

        // Gregorian
        return new DateTime(year, month, day);
    }

    public static bool TryParseWhetherGeorgianOrJalli(string input, out DateTime result)
    {
        try
        {
            result = ParseWhetherGeorgianOrJalli(input);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

}
