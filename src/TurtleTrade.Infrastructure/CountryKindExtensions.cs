using System;
using TT.StockQuoteSource.Contracts;
using TurtleTrade.Abstraction;

namespace TurtleTrade.Infrastructure
{
    public static class CountryKindExtensions
    {
        public static Country ConvertToTTStockQuoteSourceCountry(this CountryKind country)
        {
            switch (country)
            {
                case CountryKind.Taiwan:
                    return Country.Taiwan;
                case CountryKind.USA:
                    return Country.USA;
                case CountryKind.HK:
                    return Country.HK;
                default:
                    return Country.Test;
            }
        }

        public static CountryKind ConvertToTT2Country(this Country country)
        {
            switch (country)
            {
                case Country.Taiwan:
                    return CountryKind.Taiwan;
                case Country.USA:
                    return CountryKind.USA;
                case Country.HK:
                    return CountryKind.HK;
                case Country.Test:
                    return CountryKind.Test;
                default:
                    return CountryKind.Unknown;
            }
        }

        private static DateTime GetLocalTime(this CountryKind country)
        {
            DateTime result;
            switch (country)
            {
                case CountryKind.USA:
                    result = GetTime("Eastern Standard Time");
                    break;
                case CountryKind.Taiwan:
                case CountryKind.HK:
                    result = GetTime("Taipei Standard Time");
                    break;
                default:
                    result = DateTime.Now;
                    break;
            }
            return result;
        }

        ///<summary>
        /// Example: TW 08-11 12:30
        /// </summary>
        /// <param name="date"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        public static string GetShortTime(this CountryKind country)
        {
            DateTime temp = GetLocalTime(country);

            switch (country)
            {
                case CountryKind.USA:
                    return $"US {temp.ToString("MM-dd HH:mm")}";
                case CountryKind.Taiwan:
                    return $"TW {temp.ToString("MM-dd HH:mm")}";
                case CountryKind.HK:
                    return $"HK {temp.ToString("MM-dd HH:mm")}";
                default:
                    return temp.ToString("MM-dd HH:mm");
            }
        }

        public static string GetFullName(this CountryKind country)
        {
            return Enum.GetName(typeof(CountryKind), country);
        }

        public static string GetShortName(this CountryKind country)
        {

            switch (country)
            {
                case CountryKind.Taiwan:
                    return "TW";
                case CountryKind.USA:
                    return "US";
                case CountryKind.HK:
                    return "HK";
                case CountryKind.Test:
                    return "ZZ";
                case CountryKind.Test2:
                    return "ZY";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Get country kind from country short name
        /// For example, TW = Taiwan, US = USA, HK = Hong Kong
        /// </summary>
        /// <param name="s">Country short name</param>
        /// <returns></returns>
        public static CountryKind GetCountryKindFromShortName(this string s)
        {
            switch (s.ToUpper())
            {
                case "TW":
                    return CountryKind.Taiwan;
                case "US":
                    return CountryKind.USA;
                case "HK":
                    return CountryKind.HK;
                case "TEST":
                case "ZZ":
                    return CountryKind.Test;
                case "TEST2":
                case "ZY":
                    return CountryKind.Test2;
            }

            return CountryKind.Unknown;
        }

        private static DateTime GetTime(string timeZoneName)
        {
            if (string.IsNullOrWhiteSpace(timeZoneName))
            {
                throw new ArgumentNullException("TimeZoneName cannot be empty");
            }

            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);

            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, tzInfo);
        }
    }
}
