namespace TurtleTrade.Abstraction.Database
{
    public interface IMemberStock : IMember, IStockID
    {
        bool IsNotify { get; }

        BuySellStrategyType Strategy { get; set; }
    }
}
