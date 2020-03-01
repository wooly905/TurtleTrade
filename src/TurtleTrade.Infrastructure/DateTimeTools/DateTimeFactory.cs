using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.DateTimeTools
{
    public class DateTimeFactory
    {
        public static IDateTimeTool2 GenerateDateTimeTool(CountryKind country)
        {
            IDateTimeTool2 tool;
            switch (country)
            {
                case CountryKind.Taiwan:
                case CountryKind.HK:
                    tool = new TaiwanDateTimeTool();
                    break;
                default:
                    tool = new USAEastDateTimeTool();
                    break;
            }
            return tool;
        }
    }
}
