using System;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.DateTimeTools
{
    public class HKDateTimeTool : IDateTimeTool2
    {
        private readonly TimeZoneInfo _tzInfo;

        public HKDateTimeTool()
        {
            _tzInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.ChinaStandardTime);
        }

        public DateTime GetTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, _tzInfo);
        }
    }
}
