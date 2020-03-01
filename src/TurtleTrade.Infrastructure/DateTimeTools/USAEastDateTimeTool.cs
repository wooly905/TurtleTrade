using System;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.DateTimeTools
{
    public class USAEastDateTimeTool : IDateTimeTool2
    {
        private readonly TimeZoneInfo _tzInfo;

        public USAEastDateTimeTool()
        {
            _tzInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.EasternStandardTime);
        }

        public DateTime GetTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, _tzInfo);
        }
    }
}
