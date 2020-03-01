namespace TurtleTrade.Abstraction.Database
{
    public interface IStockID
    {
        CountryKind Country { get; }

        string StockId { get; }
    }
}
