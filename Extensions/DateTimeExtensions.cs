using System;
using System.Globalization;

namespace Common.Lib.Extensions
{
    public static class DateTimeExtensions
    {
        public static int Iso8601WeekOfYear(this DateTime date)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static DateTime UnixTimeStampToDateTime(this long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static long ToUnixTimeStamp(this DateTime dateTime)
        {
            long unixTimestamp = (long)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            return unixTimestamp;
        }

        public static DateTime JavaTimeStampToDateTime(this long javaTimeStamp, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            // Java timestamp is milliseconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, dateTimeKind);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static long ToJavaTimeStamp(this DateTime dateTime)
        {
            long javaTimestamp = (long)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;

            return javaTimestamp;
        }
    }
}
