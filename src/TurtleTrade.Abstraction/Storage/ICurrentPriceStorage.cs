using System.Collections.Generic;

namespace TurtleTrade.Abstraction.Storage
{
    public interface ICurrentPriceStorage
    {
        bool IsAddedOrUpdated { get; }

        void AddOrUpdateItem(CountryKind country, string stockId, ICurrentPrice item);

        IReadOnlyList<KeyValuePair<string, ICurrentPrice>> GetOrderedItems();

        bool TryGetItem(CountryKind country, string stockId, out ICurrentPrice item);
    }
}
