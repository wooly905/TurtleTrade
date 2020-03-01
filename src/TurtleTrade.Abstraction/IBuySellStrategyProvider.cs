using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;

namespace TurtleTrade.Abstraction
{
    public interface IBuySellStrategyProvider
    {
        ITradingStrategy GetBuyStrategy(IMemberStock memberStock, IBaseData baseData, bool testStatus);

        ITradingStrategy GetSellStrategy(IMemberBuyStock memberBuyStock, IBaseData baseData, bool testStatus);
    }
}
