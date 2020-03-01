using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.ServiceWorkers.BackTestWorkers
{
    internal class BackTestNotificationService : INofiticationService
    {
        private readonly List<BackTestBuySellRecord> _records;
        private readonly ITurtleLogger _logger;

        public BackTestNotificationService(ITurtleLogger logger)
        {
            _records = new List<BackTestBuySellRecord>();
            _logger = logger;
        }

        public Task SendEmailAsync(CountryKind country, DateTime time, IEmailTemplate template)
        {
            if (template == null)
            {
                return Task.CompletedTask;
            }

            try
            {
                BackTestBuySellRecord record = JsonConvert.DeserializeObject<BackTestBuySellRecord>(template.HtmlContent);
                _records.Add(record);
            }
            catch (Exception ex)
            {
                _logger?.WriteToErrorLogAsync(country, time, "BackTestNotificationService", ex);
            }

            return Task.CompletedTask;
        }

        public List<BackTestBuySellRecord> GetBuySellReocrds() => _records;
    }
}
