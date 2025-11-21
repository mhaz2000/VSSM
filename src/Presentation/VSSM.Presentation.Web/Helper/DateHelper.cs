using System;
using System.Globalization;

namespace VSSM.Presentation.Web.Helpers
{
    public static class DateHelper
    {
        public static DateTime? PersianToGregorian(int? year, int? month, int? day)
        {
            if (year == null || month == null || day == null)
                return null;

            var pc = new PersianCalendar();

            try
            {
                return pc.ToDateTime(year.Value, month.Value, day.Value, 0, 0, 0, 0);
            }
            catch
            {
                return null;
            }
        }
    }
}
