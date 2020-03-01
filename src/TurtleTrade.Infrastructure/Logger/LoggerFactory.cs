using System;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.Logger
{
    public static class LoggerFactory
    {
        private static readonly Lazy<ITurtleLogger> _loggerInstance;

#pragma warning disable CA1810 // Initialize reference type static fields inline
        static LoggerFactory() => _loggerInstance = new Lazy<ITurtleLogger>(GetLoggerInternal, true);
#pragma warning restore CA1810 // Initialize reference type static fields inline

        public static ITurtleLogger GetLogger() => _loggerInstance.Value;

        private static ITurtleLogger GetLoggerInternal()
        {
            return Environment.UserInteractive ? new ConsoleLogger() : (ITurtleLogger)new FileLogger();
        }
    }
}
