namespace TurtleTrade.Abstraction.Database
{
    public interface IStock : IStockID
    {
        string Description { get; }

        string StockExchangeID { get; }

        string StockExchangeName { get; }

        string StockName { get; }
    }
}
