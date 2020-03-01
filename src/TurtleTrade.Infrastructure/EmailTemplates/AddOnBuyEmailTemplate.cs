using System.Text;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.EmailTemplates
{
    public class AddOnBuyEmailTemplate : IEmailTemplate
    {
        private readonly string _stockID;
        private readonly string _stockName;
        private readonly decimal _newStopPrice;
        private readonly StockBuyState _newState;

        public AddOnBuyEmailTemplate(string receipentEmail, string stockID, string stockName, decimal newStopPrice, StockBuyState newState)
        {
            ReceipentEmail = receipentEmail;
            _stockID = stockID;
            _stockName = stockName;
            _newStopPrice = newStopPrice;
            _newState = newState;
        }

        public string ReceipentEmail { get; }

        public string HtmlContent
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<html>")
                  .AppendFormat($"<b>{_stockID} ({_stockName})</b> meets add-on. The new stop price is <b>{_newStopPrice}</b> and new StockBuyState is <b>{_newState.ToString()}</b>")
                  .Append("<br><br>")
                  .AppendFormat($"<b>{_stockID} ({_stockName})</b> 遇到加碼買進．新的停損價是 <b>{_newStopPrice}</b> 並且新的 StockBuyState 是 <b>{_newState.ToString()}</b>")
                  .Append("</html>");

                return sb.ToString();
            }
        }

        public string Subject => $"Turtle2 - AddBuy {_stockID} to State {_newState.ToString()}";
    }
}
