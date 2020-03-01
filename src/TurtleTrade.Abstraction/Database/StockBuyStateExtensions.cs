using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Abstraction.Database
{
    public static class StockBuyStateExtensions
    {
        public static StockBuyState GetStockBuyState(this int input)
        {
            switch (input)
            {
                case 0:
                    return StockBuyState.Buy;
                case 1:
                    return StockBuyState.FirstAdd;
                case 2:
                    return StockBuyState.SecondAdd;
                case 3:
                    return StockBuyState.ThirdAdd;
                case 4:
                    return StockBuyState.FourthAdd;
                case 99:
                    return StockBuyState.Sold;
            }

            return StockBuyState.Unknown;
        }

        public static int GetStockBuyStateValue(this StockBuyState state)
        {
            switch (state)
            {
                case StockBuyState.Buy:
                    return 0;
                case StockBuyState.FirstAdd:
                    return 1;
                case StockBuyState.SecondAdd:
                    return 2;
                case StockBuyState.ThirdAdd:
                    return 3;
                case StockBuyState.FourthAdd:
                    return 4;
                case StockBuyState.Sold:
                    return 99;
            }

            return 999;
        }
    }
}
