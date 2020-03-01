using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Database
{
    internal class MemberStock : IMemberStock
    {
        public MemberStock(string memberEmail, CountryKind country, string stockID, bool isNotify = false, BuySellStrategyType strategy = BuySellStrategyType.N20)
        {
            IsNotify = isNotify;
            MemberEmail = memberEmail;
            Country = country;
            StockId = stockID;
            Strategy = strategy;
        }

        public CountryKind Country { get; }

        public bool IsNotify { get; set; }

        public string MemberEmail { get; }

        public string StockId { get; }

        public BuySellStrategyType Strategy { get; set; }
    }
}
