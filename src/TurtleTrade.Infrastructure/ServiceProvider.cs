using System;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Config;
using TurtleTrade.Abstraction.Utilities;
using TurtleTrade.Infrastructure.DateTimeTools;

namespace TurtleTrade.Infrastructure
{
    public static class ServiceProvider
    {
        public static IBaseData CreateBaseData(CountryKind country, ITurtleLogger logger, ISystemConfig systemConfig, bool runInTestMode = false)
        {
            return new BaseData(country, logger, systemConfig, runInTestMode);
        }

        public static IBaseData CreateBaseDataWithVariableDateTimeTool(CountryKind country, DateTime startDate, ITurtleLogger logger, ISystemConfig systemConfig)
        {
            VariableDateTimeTool dateTool = new VariableDateTimeTool();
            dateTool.SetTime(startDate);

            return new BaseData(country, dateTool, logger, systemConfig);
        }
    }
}
