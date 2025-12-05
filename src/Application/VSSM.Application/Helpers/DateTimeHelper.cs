using System.Globalization;

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
}
