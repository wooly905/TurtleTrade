using System.Collections.Generic;
using TurtleTrade.Abstraction.Config;

namespace TurtleTrade.Infrastructure.Config
{
    public class SystemConfig : ISystemConfig
    {
        public SystemConfig(TradingTime[] tradingTimes, SystemInfo systemInfo, SMTPInfo smtpInfo)
        {
            TradingTimes = tradingTimes;
            SystemInfo = systemInfo;
            SMTPInfo = smtpInfo;
        }

        public ISMTPInfo SMTPInfo { get; set; }
        
        public ISystemInfo SystemInfo { get; set; }

        public IReadOnlyList<ITradingTime> TradingTimes { get; set; }
    }
}
