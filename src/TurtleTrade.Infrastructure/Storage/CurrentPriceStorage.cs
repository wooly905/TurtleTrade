using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Storage;

namespace TurtleTrade.Infrastructure.Storage
{
    public class CurrentPriceStorage : ICurrentPriceStorage
    {
        private static readonly ConcurrentDictionary<string, ICurrentPrice> _storage;

        static CurrentPriceStorage()
        {
            _storage = new ConcurrentDictionary<string, ICurrentPrice>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddOrUpdateItem(CountryKind country, string stockId, ICurrentPrice item)
        {
            string stockFullId = $"{country.GetShortName()}.{stockId}";

            if (_storage.ContainsKey(stockFullId)
                && _storage[stockFullId] == item)
            {
                return;
            }

            _storage[stockFullId] = item;
            IsAddedOrUpdated = true;
        }

        public bool IsAddedOrUpdated { get; private set; }

        public bool TryGetItem(CountryKind country, string stockId, out ICurrentPrice item)
        {
            string stockFullId = $"{country.GetShortName()}.{stockId}";

            if (_storage.TryGetValue(stockFullId, out ICurrentPrice target))
            {
                item = target;
                return true;
            }

            item = null;
            return false;
        }

        public IReadOnlyList<KeyValuePair<string, ICurrentPrice>> GetOrderedItems()
        {
            return _storage.OrderBy(k => k.Key).ToList();
        }
    }
}
