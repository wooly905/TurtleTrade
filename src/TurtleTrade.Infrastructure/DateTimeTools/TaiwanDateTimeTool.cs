using System;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.DateTimeTools
{
    public class TaiwanDateTimeTool : IDateTimeTool2
    {
        private readonly TimeZoneInfo _tzInfo;

        public TaiwanDateTimeTool()
        {
            _tzInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.TaipeiStandardTime);
        }

        public DateTime GetTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, _tzInfo);
        }
    }
}

