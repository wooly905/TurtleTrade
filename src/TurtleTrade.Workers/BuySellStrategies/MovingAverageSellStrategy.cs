using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction.Storage;
using TurtleTrade.Abstraction.Utilities;
using TurtleTrade.Infrastructure;

namespace TurtleTrade.ServiceWorkers.BuySellStrategy
{
    internal class MovingAverageSellStrategy : BuySellStrategyBase, ITradingStrategy
    {
        private readonly bool _isBackTest;
        private readonly bool _testStatus;

        public MovingAverageSellStrategy(IBaseData baseData,
                                         bool isBackTest = false,
                                         bool testStatus = false)
            : base(baseData)
        {
            _isBackTest = isBackTest;
            _testStatus = testStatus;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            IReadOnlyList<IMemberBuyStock> memberBuyStocks = await DatabaseOperations.GetMemberBuyStocksAsync().ConfigureAwait(false);

            if (memberBuyStocks == null || memberBuyStocks.Count == 0)
            {
                return;
            }

            foreach (IMemberBuyStock memberBuyStock in memberBuyStocks)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                Task t = SellNotificationInternalAsync(BaseData.Country, memberBuyStock, token);
            }
        }

        private async Task SellNotificationInternalAsync(CountryKind country, IMemberBuyStock memberBuyStock, CancellationToken token)
        {
            if (!BaseData.CurrentPriceStorage.TryGetItem(country, memberBuyStock.StockId, out ICurrentPrice targetItem)
                || targetItem == null
                || !BaseData.CurrentTime.IsSameDay(targetItem.LastTradeTime)
                || token.IsCancellationRequested)
            {
                return;
            }

            string stockFullId = $"{country.GetShortName()}.{memberBuyStock.StockId}";
            decimal? previousMovingAveragePrice = await GetBreakDownComparedPriceAsync(stockFullId, memberBuyStock.Strategy).ConfigureAwait(false);

            if (previousMovingAveragePrice == null || token.IsCancellationRequested)
            {
                return;
            }

            // TODO : refactor
            if (!PriceNotificationChecker.CanNotify(memberBuyStock.MemberEmail,
                                                   stockFullId,
                                                   StockNotificationType.SellStop,
                                                   BaseData.CurrentTime,
                                                   memberBuyStock.Strategy,
                                                   StockBuyState.Sold))
            {
                return;
            }

            // when CurrentPrice < (LowIn10 or StopPrice), notify the member
            string stockName = await GetStockNameAsync(memberBuyStock.StockId).ConfigureAwait(false);
            IEmailTemplate emailTemplate = null;

            if (targetItem.CurrentPrice < (previousMovingAveragePrice.Value - (2 * memberBuyStock.NValue)))
            {
                // lower than previous MA price - 2*N
                emailTemplate = EmailTemplateProvider.GetBreakDownEmailTemplate(memberBuyStock.MemberEmail,
                                                                                stockFullId,
                                                                                stockName,
                                                                                memberBuyStock.Strategy,
                                                                                previousMovingAveragePrice.Value,
                                                                                previousMovingAveragePrice.Value);
            }
            else if (targetItem.CurrentPrice < memberBuyStock.StopPrice)
            {
                // lower than stop price
                emailTemplate = EmailTemplateProvider.GetStopLossEmailTemplate(memberBuyStock.MemberEmail,
                                                                               stockFullId,
                                                                               stockName,
                                                                               memberBuyStock.Strategy,
                                                                               memberBuyStock.StopPrice,
                                                                               memberBuyStock.StopPrice);
            }

            if (emailTemplate == null)
            {
                return;
            }

            if (_testStatus)
            {
                await EmailService.SendEmailAsync(country, BaseData.CurrentTime, emailTemplate).ConfigureAwait(false);
            }
            else
            {
                await EmailService.SendEmailAsync(country, BaseData.CurrentTime, emailTemplate).ConfigureAwait(false);
            }

            PriceNotificationChecker.InsertRecord(memberBuyStock.MemberEmail,
                                                  stockFullId,
                                                  StockNotificationType.SellStop,
                                                  BaseData.CurrentTime,
                                                  memberBuyStock.Strategy,
                                                  StockBuyState.Sold);

            if (_isBackTest)
            {
                await DatabaseOperations.DeleteMemberBuyStockAsync(memberBuyStock.MemberEmail,
                                                                   memberBuyStock.Country,
                                                                   memberBuyStock.StockId).ConfigureAwait(false);
            }
        }
    }
}
