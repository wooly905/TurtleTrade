using System.Text;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.EmailTemplates
{
    public class StopLossEmailTemplate : IEmailTemplate
    {
        private readonly string _stockID;
        private readonly string _stockName;
        private readonly decimal _stopPriceInStrategy;
        private readonly decimal _currentLowPrice;
        private readonly BuySellStrategyType _strategy;

        public StopLossEmailTemplate(string receipent, string stockID, string stockName, BuySellStrategyType strategy, decimal stopPriceInStrategy, decimal currentLowPrice)
        {
            ReceipentEmail = receipent;
            _stockID = stockID;
            _stockName = stockName;
            _stopPriceInStrategy = stopPriceInStrategy;
            _currentLowPrice = currentLowPrice;
            _strategy = strategy;
        }

        public string HtmlContent
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<html>");
                sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> breaks down stop price <b>{_stopPriceInStrategy}</b><br>Current lowest price is <b>{_currentLowPrice}</b>");
                sb.Append("<br><br>");
                sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> 已跌破停損價 <b>{_stopPriceInStrategy}</b><br>目前最低價 <b>{_currentLowPrice}</b>");
                sb.Append("</html>");
                return sb.ToString();
            }
        }

        public string ReceipentEmail { get; }

        public string Subject => $"Turtle2 - Sell (stop loss) {_stockID} - system {_strategy.GetKind().ToString()} {_strategy.GetString()}";
    }
}
