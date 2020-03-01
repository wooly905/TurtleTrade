using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Abstraction.Database
{
    public static class HistoricalDataWaitingStateExtension
    {
        public static int GetStateValue(this HistoricalDataWaitingState state)
        {
            int result = -1;

            switch (state)
            {
                case HistoricalDataWaitingState.Done:
                    result = 99;
                    break;
                case HistoricalDataWaitingState.Waiting:
                    result = 0;
                    break;
                case HistoricalDataWaitingState.Working:
                    result = 1;
                    break;
            }

            return result;
        }
    }
}
