using System;

namespace TurtleTrade.Abstraction.Utilities
{
    [Obsolete("Use new interface - ITurtleMessenger")]
    public interface ITurtleNotification
    {
        void SemdEmail(IEmailTemplate template);
    }
}
