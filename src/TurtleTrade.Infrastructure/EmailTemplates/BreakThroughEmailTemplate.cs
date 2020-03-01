using System.Text;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.EmailTemplates
{
    public class BreakThroughEmailTemplate : IEmailTemplate
    {
        private readonly string _stockID;
        private readonly string _stockName;
        private readonly decimal _todayHighPrice;
        private readonly decimal _highPriceInStrategy;
        private readonly BuySellStrategyType _strategy;

        public BreakThroughEmailTemplate(string receipent, string stockID, string stockName, BuySellStrategyType strategy, decimal todayHighPrice, decimal highPriceInStrategy)
        {
            ReceipentEmail = receipent;
            _stockID = stockID;
            _stockName = stockName;
            _todayHighPrice = todayHighPrice;
            _highPriceInStrategy = highPriceInStrategy;
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
                    sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> breaks through highest price in {_strategy.GetString()} days <b>{_highPriceInStrategy}</b><br>Today's highest price is {_todayHighPrice}");
                    sb.Append("<br><br>");
                    sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> 突破 {_strategy.GetString()} 日最高價 <b>{_highPriceInStrategy}</b><br>今天最高價: <b>{_todayHighPrice}</b></html>");
                }
                else if (_strategy.GetKind() == BuySellStrategyKind.MovingAverage)
                {
                    sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> breaks through the price <b>{_highPriceInStrategy}</b> in {_strategy.GetKind().ToString()} {_strategy.GetString()} strategy <br>Today's highest price is {_todayHighPrice}");
                    sb.Append("<br><br>");
                    sb.AppendFormat($"<b>{_stockID} ({_stockName})</b> 突破 {_strategy.GetKind().ToString()} {_strategy.GetString()} 策略的高價 <b>{_highPriceInStrategy}</b><br>今天最高價: <b>{_todayHighPrice}</b></html>");
                }

                sb.Append("</html>");
                return sb.ToString();
            }
        }

        public string ReceipentEmail { get; }

        public string Subject
        {
            get
            {
                if (_strategy.GetKind() == BuySellStrategyKind.Turtle)
                {
                    return $"Turtle2 - FirstBuy {_stockID} - System {_strategy.GetKind().ToString()} {_strategy.GetString()}";
                }
                else if (_strategy.GetKind() == BuySellStrategyKind.MovingAverage)
                {
                    return $"Turtle2 - Buy {_stockID} - System {_strategy.GetKind().ToString()} {_strategy.GetString()}";
                }

                return "No Subject";
            }
        }
        
       
    }
}
