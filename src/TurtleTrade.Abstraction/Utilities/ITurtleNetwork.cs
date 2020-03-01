using System;

namespace TurtleTrade.Abstraction.Utilities
{
    [Obsolete("Use new interface - ITurtleBrokerProxy")]
    public interface ITurtleNetwork
    {
        string GoogleSendRequest(string googleHttpData);

        string YahooSendRequest(string yahooHttpData);
    }
}
