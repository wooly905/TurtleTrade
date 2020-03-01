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
    internal class TurtleSellStrategy : BuySellStrategyBase, ITradingStrategy
    {
        private readonly bool _isBackTest;
        private readonly bool _testStatus;
        private readonly IMemberBuyStock _memberBuyStock;

        public TurtleSellStrategy(IMemberBuyStock memberBuyStock,
                                  IBaseData baseData,
                                  bool isBackTest = false,
                                  bool testStatus = false)
            : base(baseData)
        {
            _isBackTest = isBackTest;
            _memberBuyStock = memberBuyStock;
            _testStatus = testStatus;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            if (_memberBuyStock == null)
            {
                return;
            }

            string stockFullId = $"{_memberBuyStock.Country.GetShortName()}.{_memberBuyStock.StockId}";

            if (!BaseData.CurrentPriceStorage.TryGetItem(_memberBuyStock.Country, _memberBuyStock.StockId, out ICurrentPrice target)
                || target == null
                || !BaseData.CurrentTime.IsSameDay(target.LastTradeTime)
                || token.IsCancellationRequested)
            {
                return;
            }

            decimal currentPriceFromStorage = target.CurrentPrice;
            decimal? previousLowPriceInStrategy = await GetBreakDownComparedPriceAsync(stockFullId, _memberBuyStock.Strategy).ConfigureAwait(false);

            if (previousLowPriceInStrategy == null || token.IsCancellationRequested)
            {
                return;
            }

            // TODO : refactor
            if (!PriceNotificationChecker.CanNotify(_memberBuyStock.MemberEmail,
                                                   stockFullId,
                                                   StockNotificationType.SellStop,
                                                   BaseData.CurrentTime,
                                                   _memberBuyStock.Strategy,
                                                   StockBuyState.Sold))
            {
                return;
            }

            // when CurrentPrice < (LowIn10 or StopPrice), notify the member
            string stockName = await GetStockNameAsync(_memberBuyStock.StockId).ConfigureAwait(false);
            IEmailTemplate emailTemplate = null;

            if (currentPriceFromStorage < previousLowPriceInStrategy.Value)
            {
                // lower than the price within 10 days
                emailTemplate = EmailTemplateProvider.GetBreakDownEmailTemplate(_memberBuyStock.MemberEmail,
                                                                                stockFullId,
                                                                                stockName,
                                                                                _memberBuyStock.Strategy,
                                                                                previousLowPriceInStrategy.Value,
                                                                                previousLowPriceInStrategy.Value);
            }
            else if (currentPriceFromStorage < _memberBuyStock.StopPrice)
            {
                // lower than stop price
                emailTemplate = EmailTemplateProvider.GetStopLossEmailTemplate(_memberBuyStock.MemberEmail,
                                                                               stockFullId,
                                                                               stockName,
                                                                               _memberBuyStock.Strategy,
                                                                               _memberBuyStock.StopPrice,
                                                                               _memberBuyStock.StopPrice);
            }

            if (emailTemplate == null)
            {
                return;
            }

            if (_testStatus)
            {
                await EmailService.SendEmailAsync(BaseData.Country, BaseData.CurrentTime, emailTemplate).ConfigureAwait(false);
            }
            else
            {
                await EmailService.SendEmailAsync(BaseData.Country, BaseData.CurrentTime, emailTemplate).ConfigureAwait(false);
            }

            PriceNotificationChecker.InsertRecord(_memberBuyStock.MemberEmail,
                                                  stockFullId,
                                                  StockNotificationType.SellStop,
                                                  BaseData.CurrentTime,
                                                  _memberBuyStock.Strategy,
                                                  StockBuyState.Sold);

            if (_isBackTest)
            {
                await DatabaseOperations.DeleteMemberBuyStockAsync(_memberBuyStock.MemberEmail,
                                                                   _memberBuyStock.Country,
                                                                   _memberBuyStock.StockId).ConfigureAwait(false);
            }
        }
    }
}
