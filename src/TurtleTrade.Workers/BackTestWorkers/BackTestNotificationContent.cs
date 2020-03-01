using System;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.ServiceWorkers.BackTestWorkers
{
    internal class BackTestNotificationContent : IEmailTemplate
    {

        public BackTestNotificationContent(string stockId,
                                           string action,
                                           string op,
                                           decimal price,
                                           DateTime actionDate,
                                           string comment = "")
        {
            string date = actionDate.ToString("yyyy-MM-dd");
            HtmlContent = string.Concat($"{{\"StockId\":\"{stockId}\",",
                                        $"\"Action\":\"{action}\",",
                                        $"\"Op\":\"{op}\",",
                                        $"\"Price\":{price},",
                                        $"\"Date\":\"{date}\",",
                                        $"\"Comment\":\"{comment}\"}}");
        }

        public string ReceipentEmail => string.Empty;

        public string HtmlContent { get; }

        public string Subject => string.Empty;
    }
}
