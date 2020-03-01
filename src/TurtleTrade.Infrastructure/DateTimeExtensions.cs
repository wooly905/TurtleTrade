using System;
using System.Globalization;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure
{
    public static class DateTimeExtensions
    {
        public static bool IsSameDay(this DateTime date, DateTime compare)
        {
            return date.Year == compare.Year && date.Month == compare.Month && date.Day == compare.Day;
        }

        public static DateTime GetDateAtStartOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }

        public static DateTime GetDateAtEndOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }

        public static string ToUnixTimestampe(this DateTime date)
        {
            DateTime unixStartDate = new DateTime(1970, 1, 1, 0, 0, 0);
            return (date - unixStartDate).TotalSeconds.ToString(CultureInfo.InvariantCulture);
        }

        public static DateTime ConvertUSAEastTimeToTaiwanTime(this DateTime usaEastTime)
        {
            TimeZoneInfo usaEastTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.EasternStandardTime);
            TimeZoneInfo taiwanTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.TaipeiStandardTime);

            return TimeZoneInfo.ConvertTime(usaEastTime, usaEastTimeZone, taiwanTimeZone);
        }
    }
}
