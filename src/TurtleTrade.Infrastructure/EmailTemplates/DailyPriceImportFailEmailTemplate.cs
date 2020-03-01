using System.Collections.Generic;
using System.Text;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.EmailTemplates
{
    public class DailyPriceImportFailEmailTemplate : IEmailTemplate
    {
        private IList<(string, string)> _failedData;
        private readonly string _subject;

        public DailyPriceImportFailEmailTemplate(string receipentEmail, IList<(string, string)> failedData, string subjectText = "")
        {
            ReceipentEmail = receipentEmail;
            _failedData = failedData;
            _subject = subjectText;
        }

        public string HtmlContent
        {
            get
            {
                if (_failedData == null || _failedData.Count == 0)
                {
                    return "<html><b>今天股票資料全部成功匯入</b></html>";
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<html><b>今天的 DailyPriceImport 完成，下列為今天的匯入失敗 stock symbol<b><br>");
                sb.AppendLine("<table cellspacing=1 cellpadding=1>");

                foreach ((string StockFullId, string ErrorMessage) in _failedData)
                {
                    sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", StockFullId, ErrorMessage);
                }

                sb.AppendLine("</table></html>");

                return sb.ToString();
            }
        }

        public string ReceipentEmail { get; set; }

        public string Subject => string.IsNullOrEmpty(_subject) ? "Turtle2 - DailyPriceImport 通知" : _subject;
    }
}
