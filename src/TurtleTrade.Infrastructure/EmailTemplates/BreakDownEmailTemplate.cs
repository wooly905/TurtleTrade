using System.Text;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.EmailTemplates
{
    public class BreakDownEmailTemplate : IEmailTemplate
    {
        private readonly string _stockID;
        private readonly string _stockName;
        private readonly decimal _stopPriceInStrategy;
        private readonly decimal _todayLowestPrice;
        private readonly BuySellStrategyType _strategy;

        public BreakDownEmailTemplate(string receipent, string stockID, string stockName, BuySellStrategyType strategy, decimal stopPriceInStrategy, decimal currentLowPrice)
        {
            ReceipentEmail = receipent;
            _stockID = stockID;
            _stockName = stockName;
            _stopPriceInStrategy = stopPriceInStrategy;
            _todayLowestPrice = currentLowPrice;
            _strategy = strategy;
        }

        public string HtmlContent
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<html>");

                if (_strategy.GetKind() == BuySellStrategyKind.Turtle)
                {
                    sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> breaks down lowest price in <b>{_strategy.GetString()}</b> days <b>{_stopPriceInStrategy}</b><br>Current lowest price is <b>{_todayLowestPrice}</b>");
                    sb.Append("<br><br>");
                    sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> 已跌破 <b>{_strategy.GetString()}</b> 曰最低價 <b>{_stopPriceInStrategy}</b><br>目前最低價 <b>{_todayLowestPrice}</b>");
                }
                else if (_strategy.GetKind() == BuySellStrategyKind.MovingAverage)
                {
                    sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> breaks down moving average price in <b>{_strategy.GetString()}</b> strategy <b>{_stopPriceInStrategy}</b><br>Current lowest price is <b>{_todayLowestPrice}</b>");
                    sb.Append("<br><br>");
                    sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> 已跌破 <b>{_strategy.GetString()}</b> 策略的低價 <b>{_stopPriceInStrategy}</b><br>目前最低價 <b>{_todayLowestPrice}</b>");
                }

                sb.Append("</html>");
                return sb.ToString();
            }
        }

        public string ReceipentEmail { get; }

        public string Subject => $"Turtle2 - Sell (break down) {_stockID} - Strategy {_strategy.GetKind().ToString()} {_strategy.GetString()}";

    }
}
