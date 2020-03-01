using System;
using TurtleTrade.Abstraction;

namespace TurtleTrade.Abstraction
{
    public static class BuySellStrategyTypeExtension
    {
        public static BuySellStrategyType GetStockStrategy(this string value)
        {
            if (string.Equals(value, "N20", System.StringComparison.OrdinalIgnoreCase))
            {
                return BuySellStrategyType.N20;
            }
            else if (string.Equals(value, "N40", System.StringComparison.OrdinalIgnoreCase))
            {
                return BuySellStrategyType.N40;
            }
            else if (string.Equals(value, "N60", System.StringComparison.OrdinalIgnoreCase))
            {
                return BuySellStrategyType.N60;
            }
            else if (string.Equals(value, "MA20", System.StringComparison.OrdinalIgnoreCase))
            {
                return BuySellStrategyType.MA20;
            }
            else if (string.Equals(value, "MA40", System.StringComparison.OrdinalIgnoreCase))
            {
                return BuySellStrategyType.MA40;
            }
            else if (string.Equals(value, "MA60", System.StringComparison.OrdinalIgnoreCase))
            {
                return BuySellStrategyType.MA60;
            }
            else if (string.Equals(value, "MA120", System.StringComparison.OrdinalIgnoreCase))
            {
                return BuySellStrategyType.MA120;
            }
            else if (string.Equals(value, "MA240", System.StringComparison.OrdinalIgnoreCase))
            {
                return BuySellStrategyType.MA240;
            }

            return BuySellStrategyType.Unknown;
        }

        public static string GetString(this BuySellStrategyType strategy)
        {
            return Enum.GetName(typeof(BuySellStrategyType), strategy);
        }

        public static int GetBuyIntValue(this BuySellStrategyType strategy)
        {
            if (strategy == BuySellStrategyType.N20 || strategy == BuySellStrategyType.MA20)
            {
                return 20;
            }

            if (strategy == BuySellStrategyType.N40 || strategy == BuySellStrategyType.MA40)
            {
                return 40;
            }

            if (strategy == BuySellStrategyType.N60 || strategy == BuySellStrategyType.MA60)
            {
                return 60;
            }

            if (strategy == BuySellStrategyType.MA120)
            {
                return 120;
            }

            if (strategy == BuySellStrategyType.MA240)
            {
                return 240;
            }

            return 0;
        }

        public static int GetSellIntValue(this BuySellStrategyType strategy)
        {
            if (strategy == BuySellStrategyType.N20)
            {
                return 10;
            }

            if (strategy == BuySellStrategyType.N40)
            {
                return 15;
            }

            if (strategy == BuySellStrategyType.N60)
            {
                return 20;
            }

            return 0;
        }

        public static BuySellStrategyKind GetKind(this BuySellStrategyType strategy)
        {
            if (strategy == BuySellStrategyType.N20
                || strategy == BuySellStrategyType.N40
                || strategy == BuySellStrategyType.N60)
            {
                return BuySellStrategyKind.Turtle;
            }
            else if (strategy == BuySellStrategyType.MA20
                     || strategy == BuySellStrategyType.MA40
                     || strategy == BuySellStrategyType.MA60
                     || strategy == BuySellStrategyType.MA120
                     || strategy == BuySellStrategyType.MA240)
            {
                return BuySellStrategyKind.MovingAverage;
            }

            return BuySellStrategyKind.Unknown;
        }
    }
}
