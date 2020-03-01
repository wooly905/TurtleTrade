using System;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Database
{
    internal class MemberBuyStock : IMemberBuyStock
    {
        public MemberBuyStock(string memberEmail, CountryKind country, string stockID, decimal buyPrice, decimal N, decimal stopPrice, StockBuyState state, BuySellStrategyType strategy, DateTime buyDate)
        {
            MemberEmail = memberEmail;
            Country = country;
            StockId = stockID;
            BuyPrice = buyPrice;
            StopPrice = stopPrice;
            State = state;
            BuyDate = buyDate;
            NValue = N;
            Strategy = strategy;
        }

        public DateTime BuyDate { get; private set; }

        public decimal BuyPrice { get; }

        public CountryKind Country { get; }

        public string MemberEmail { get; }

        public decimal NValue { get; }

        public StockBuyState State { get; internal set; }

        public string StockId { get; }

        public decimal StopPrice { get; internal set; }

        public BuySellStrategyType Strategy { get; }
    }
}
