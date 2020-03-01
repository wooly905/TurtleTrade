using System;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.DateTimeTools
{
    public class VariableDateTimeTool : IDateTimeTool2
    {
        private DateTime _currentDate;

        public void SetTime(DateTime currentDate)
        {
            _currentDate = currentDate;
        }

        public DateTime GetTime()
        {
            return _currentDate;
        }
    }
}
