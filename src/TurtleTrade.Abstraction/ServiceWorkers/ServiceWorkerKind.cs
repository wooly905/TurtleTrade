namespace TurtleTrade.Abstraction.ServiceWorkers
{
    public enum ServiceWorkerKind
    {
        Unknown = -1,
        BuyWorker = 0,
        CurrentPriceWorker = 1,
        DailyPriceWorker = 2,
        SellWorker = 3,
        StorageDumpWorker = 4,
        HistoricalPriceWorker = 5,
    }
}
