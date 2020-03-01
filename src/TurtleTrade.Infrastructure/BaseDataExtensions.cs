using System;
using System.Linq;
using TurtleTrade.Abstraction;

namespace TurtleTrade.Infrastructure
{
    public static class BaseDataProviderExtensions
    {
        public static decimal CalculateHighestPrice(this IBaseData provider, decimal[] highestInPreviousDays, decimal highestInToday)
        {
            return Math.Max(highestInPreviousDays.Max(), highestInToday);
        }

        public static decimal CalculateLowestPrice(this IBaseData provider, decimal[] lowPriceInPreviousDays, decimal lowPriceToday)
        {
            return Math.Min(lowPriceInPreviousDays.Min(), lowPriceToday);
        }

        public static decimal CalculateTodayN(this IBaseData provider, decimal[] latestATRs, decimal todayATR)
        {
            return (latestATRs.Sum() + todayATR) / (latestATRs.Length + 1);
        }

        public static decimal CalculateTodayATR(this IBaseData provider, decimal todayHighPrice, decimal todayLowPrice, decimal previousClosePrice)
        {
            // 要取絕對值之後，再選出差距最大者
            decimal temp1 = Math.Abs(todayHighPrice - todayLowPrice);
            decimal temp2 = Math.Abs(todayHighPrice - previousClosePrice);
            decimal temp3 = Math.Abs(previousClosePrice - todayLowPrice);

            return Math.Max(temp3, Math.Max(temp1, temp2));
        }

        /// <summary>
        /// True when currentTime is in between startTime and endTime (same day only); otherwise false.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static bool IsTimeInBetween(this IBaseData provider, DateTime startTime, DateTime endTime, DateTime currentTime)
        {
            return startTime <= currentTime && currentTime <= endTime;
        }
    }
}
