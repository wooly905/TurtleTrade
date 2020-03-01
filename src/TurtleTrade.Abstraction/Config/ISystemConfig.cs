using System.Collections.Generic;
using TurtleTrade.Abstraction.Config;

namespace TurtleTrade.Abstraction.Config
{
    public interface ISystemConfig
    {
        ISMTPInfo SMTPInfo { get; set; }

        ISystemInfo SystemInfo { get; set; }

        IReadOnlyList<ITradingTime> TradingTimes { get; set; }
    }
}
