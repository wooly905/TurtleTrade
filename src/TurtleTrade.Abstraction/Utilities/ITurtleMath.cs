namespace TurtleTrade.Abstraction.Utilities
{
    public interface ITurtleMath
    {
        decimal CalculateHighestIn20Days(decimal[] HighestInPrevious19Days, decimal HighestInToday);

        decimal CalculateLowestIn10Days(decimal[] LowestInPrevious9Days, decimal LowestInToday);

        decimal CalculateTodayN(decimal[] latestATRs, decimal todayATR);

        decimal DetermineTodayATR(decimal todayHighPrice, decimal todayLowPrice, decimal previousClosedPrice);
    }
}
