using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.Utilities;
using TurtleTrade.Infrastructure.EmailTemplates;

namespace TurtleTrade.ServiceWorkers.BuySellStrategy
{
    public class BuySellStrategyFactory : IBuySellStrategyProvider
    {
        private readonly IEmailTemplateProvider _emailTemplateProvider;

        public BuySellStrategyFactory()
        {
            // TODO : move to other place
            _emailTemplateProvider = new EmailTemplateProvider();
        }

        public ITradingStrategy GetBuyStrategy(IMemberStock memberStock, IBaseData baseData, bool testStatus = false)
        {
            if (memberStock.Strategy == BuySellStrategyType.N20
                || memberStock.Strategy == BuySellStrategyType.N40
                || memberStock.Strategy == BuySellStrategyType.N60)
            {
                return new TurtleBuyStrategy(memberStock, baseData, testStatus: testStatus);
            }

            if (memberStock.Strategy == BuySellStrategyType.MA20
                || memberStock.Strategy == BuySellStrategyType.MA40
                || memberStock.Strategy == BuySellStrategyType.MA60)
            {
                return new MovingAverageBuyStrategy(baseData, testStatus: testStatus);
            }

            return null;
        }

        public ITradingStrategy GetSellStrategy(IMemberBuyStock memberBuyStock, IBaseData baseData, bool testStatus = false)
        {
            if (memberBuyStock.Strategy == BuySellStrategyType.N20
                || memberBuyStock.Strategy == BuySellStrategyType.N40
                || memberBuyStock.Strategy == BuySellStrategyType.N60)
            {
                return new TurtleSellStrategy(memberBuyStock, baseData, testStatus: testStatus);
            }

            if (memberBuyStock.Strategy == BuySellStrategyType.MA20
                || memberBuyStock.Strategy == BuySellStrategyType.MA40
                || memberBuyStock.Strategy == BuySellStrategyType.MA60)
            {
                return new MovingAverageSellStrategy(baseData, testStatus: testStatus);
            }

            return null;
        }
    }
}
