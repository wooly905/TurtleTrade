using System;
using System.Linq;
using TurtleTrade.Abstraction.Config;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction;

namespace TurtleTrade.ServiceWorkers
{
    internal static class BaseDataExtensions
    {
        public static DateTime GetWorkerStartTime(this IBaseData baseData, ServiceWorkerKind kind)
        {
            return GetWorkerTime(baseData, kind, true);
        }

        public static DateTime GetWorkerEndTime(this IBaseData baseData, ServiceWorkerKind kind)
        {
            return GetWorkerTime(baseData, kind, false);
        }

        public static ITradingTime GetCountryTradingTimes(this IBaseData baseData, CountryKind country)
        {
            return baseData.SystemConfig.TradingTimes.FirstOrDefault(c => c.Country == country);
        }

        private static DateTime GetWorkerTime(IBaseData baseData, ServiceWorkerKind kind, bool start)
        {
            switch (kind)
            {
                case ServiceWorkerKind.BuyWorker:
                case ServiceWorkerKind.CurrentPriceWorker:
                case ServiceWorkerKind.SellWorker:
                case ServiceWorkerKind.StorageDumpWorker:
                    if (start)
                    {
                        DateTime? tradeStartTime = baseData.GetCountryTradingTimes(baseData.Country)?.TradingStartTime;

                        if (tradeStartTime != null)
                        {
                            return tradeStartTime.Value;
                        }
                    }
                    else
                    {
                        DateTime? tradeEndTime = baseData.GetCountryTradingTimes(baseData.Country)?.TradingEndTime;

                        if (tradeEndTime != null)
                        {
                            return tradeEndTime.Value;
                        }
                    }
                    break;
                case ServiceWorkerKind.DailyPriceWorker:
                    DateTime? dailyPriceImportTime = baseData.GetCountryTradingTimes(baseData.Country)?.DailyPriceImportTime;

                    if (dailyPriceImportTime != null)
                    {
                        return dailyPriceImportTime.Value;
                    }
                    break;
            }

            if (start)
            {
                // for test, always return 0:0:0
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }

            // for test, always return 23:59:59
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
        }
    }
}
