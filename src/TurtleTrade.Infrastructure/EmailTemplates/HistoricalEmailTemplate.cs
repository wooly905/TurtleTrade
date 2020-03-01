using System.Collections.Generic;
using System.Text;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.EmailTemplates
{
    public class HistoricalEmailTemplate : IEmailTemplate
    {
        private readonly IReadOnlyList<IHistoricalDataWaitingEntry> _entries;

        public HistoricalEmailTemplate(string receipentEmail, IReadOnlyList<IHistoricalDataWaitingEntry> entries)
        {
            ReceipentEmail = receipentEmail;
            _entries = entries;
        }

        public string ReceipentEmail { get; }

        public string HtmlContent
        {
            get
            {
                if (_entries == null || _entries.Count == 0)
                {
                    return "沒有任何股票完成 Historical data import";
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("<html>");
                sb.AppendLine("<h3>下列股票完成 historical data import</h3><br>");
                sb.AppendLine("<table cellspacing=1 cellpadding=0 border=1>");

                foreach (IHistoricalDataWaitingEntry entry in _entries)
                {
                    sb.Append("<tr><td>")
                      .Append(entry.StockId)
                      .Append("</td><td>")
                      .AppendFormat("{0:yyyy-MM-dd}", entry.DataStartDate)
                      .Append("</td><td>")
                      .AppendFormat("{0:yyyy-MM-dd}", entry.DataEndDate)
                      .AppendLine("</td></tr>");
                }

                sb.AppendLine("</table></html>");

                return sb.ToString();
            }
        }

        public string Subject => "TT2 - Historical data worker notification";
    }
}
