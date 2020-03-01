namespace TurtleTrade.Abstraction.Database
{
    public enum HistoricalDataWaitingState
    {
        Unknown = -1,
        Waiting = 0,
        Working = 1,
        Done = 99
    }
}
