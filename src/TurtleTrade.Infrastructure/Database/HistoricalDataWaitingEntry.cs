using System;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Database
{
    public class HistoricalDataWaitingEntry : IHistoricalDataWaitingEntry
    {
        public HistoricalDataWaitingEntry(CountryKind country, string stockId, HistoricalDataWaitingState state, DateTime startDate, DateTime endDate)
        {
            Country = country;
            StockId = stockId;
            State = state;
            DataStartDate = startDate;
            DataEndDate = endDate;
        }

        public HistoricalDataWaitingEntry(CountryKind country, string stockId, int state, DateTime startDate, DateTime endDate)
        {
            Country = country;
            StockId = stockId;
            State = ConvertState(state);
            DataStartDate = startDate;
            DataEndDate = endDate;
        }

        private HistoricalDataWaitingState ConvertState(int input)
        {
            switch (input)
            {
                case -1:
                    return HistoricalDataWaitingState.Unknown;
                case 0:
                    return HistoricalDataWaitingState.Waiting;
                case 1:
                    return HistoricalDataWaitingState.Working;
                case 99:
                    return HistoricalDataWaitingState.Done;
            }

            return HistoricalDataWaitingState.Unknown;
        }

        public HistoricalDataWaitingState State { get; set; }

        public DateTime DataStartDate { get; private set; }

        public DateTime DataEndDate { get; private set; }

        public CountryKind Country { get; private set; }

        public string StockId { get; private set; }
    }
}
