using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class TurtleBuyStrategy : BuySellStrategyBase, ITradingStrategy
    {
        private readonly bool _isBackTest;
        private readonly IMemberStock _memberStock;
        private readonly bool _testStatus;

        public TurtleBuyStrategy(IMemberStock memberStock,
                                 IBaseData baseData,
                                 bool isBackTest = false,
                                 bool testStatus = false)
            : base(baseData)
        {
            _isBackTest = isBackTest;
            _memberStock = memberStock;
            _testStatus = testStatus;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            if (_memberStock == null)
            {
                return;
            }

            Task t1 = FirstBuyAsync(token);
            Task t2 = AddBuyAsync(token);

            try
            {
                await Task.WhenAll(t1, t2).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!_testStatus)
                {
                    Task t = BaseData.GetLogger().WriteToErrorLogAsync(BaseData.Country, BaseData.CurrentTime, "TurtleBuyStrategy", ex);
                }
            }
        }

        // add records into database
        private async Task BackTestFirstBuyOperationAsync(decimal buyPrice)
        {
            IReadOnlyList<IStockPriceHistory> items = await DatabaseOperations.GetStockPriceHistoryAsync(_memberStock.Country, _memberStock.StockId, BaseData.CurrentTime, 1).ConfigureAwait(false);

            if (items == null || items.Count == 0)
            {
                if (!_testStatus)
                {
                    _ = BaseData.GetLogger().WriteToErrorLogAsync(BaseData.Country, BaseData.CurrentTime, "TurtleBuyStrategy", new Exception($"BuyStockRunner: No stock price history data on previous business day of {BaseData.CurrentTime:yyyy-MM-dd}"));
                }

                return;
            }

            IStockPriceHistory item = items.FirstOrDefault();

            if (item == null)
            {
                return;
            }

            decimal N = -1m;

            // TODO : refactor
            switch (_memberStock.Strategy)
            {
                case BuySellStrategyType.N20:
                    if (item.N20 != null)
                    {
                        N = item.N20.Value;
                    }

                    break;
                case BuySellStrategyType.N40:
                    if (item.N40 != null)
                    {
                        N = item.N40.Value;
                    }

                    break;
                case BuySellStrategyType.N60:
                    if (item.N60 != null)
                    {
                        N = item.N60.Value;
                    }

                    break;
            }

            if (N == -1m)
            {
                return;
            }

            await DatabaseOperations.AddMemberBuyStockAsync(_memberStock.MemberEmail,
                                                            _memberStock.Country,
                                                            _memberStock.StockId,
                                                            buyPrice,
                                                            N,
                                                            _memberStock.Strategy,
                                                            BaseData.CurrentTime).ConfigureAwait(false);
        }

        private async Task FirstBuyAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            IReadOnlyList<IMemberStock> memberStockList = await GetTurtleStrategyMemberStockListAsync(_memberStock.MemberEmail).ConfigureAwait(false);

            if (memberStockList == null || memberStockList.Count == 0)
            {
                return;
            }

            foreach (IMemberStock memberStock in memberStockList)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                Task t = FirstBuyInternalAsync(memberStock, BaseData.CurrentPriceStorage);
            }
        }

        private async Task FirstBuyInternalAsync(IMemberStock memberStock, ICurrentPriceStorage priceStorage)
        {
            if (!priceStorage.TryGetItem(memberStock.Country, memberStock.StockId, out ICurrentPrice target)
                || target == null
                || !BaseData.CurrentTime.IsSameDay(target.LastTradeTime))
            {
                return;
            }

            decimal todayHigh = target.TodayHighPrice;
            string stockFullId = $"{memberStock.Country.GetShortName()}.{memberStock.StockId}";
            decimal? previousHighBoundPrice = await GetBreakThroughComparedPriceAsync(stockFullId, memberStock.Strategy).ConfigureAwait(false);

            // check if breaks through high price in N system
            if (!previousHighBoundPrice.HasValue || previousHighBoundPrice.Value >= todayHigh)
            {
                return;
            }

            // check if this member bought the stock. if bought, no notification.
            bool isUserHold = await IfUserHoldStock(_memberStock.MemberEmail, stockFullId).ConfigureAwait(false);

            if (isUserHold)
            {
                return;
            }

            // check if notify today  
            if (!PriceNotificationChecker.CanNotify(_memberStock.MemberEmail,
                                                   stockFullId,
                                                   StockNotificationType.Buy,
                                                   BaseData.CurrentTime,
                                                   _memberStock.Strategy,
                                                   StockBuyState.Buy))
            {
                return;
            }

            string stockName = await GetStockNameAsync(stockFullId).ConfigureAwait(false);
            // should change to some EmailTemplateProvider
            IEmailTemplate emailMessage = EmailTemplateProvider.GetBreakThroughEmailTemplate(_memberStock.MemberEmail,
                                                                                             stockFullId,
                                                                                             stockName,
                                                                                             _memberStock.Strategy,
                                                                                             todayHigh,
                                                                                             previousHighBoundPrice ?? -1);
            if (_testStatus)
            {
                _ = EmailService.SendEmailAsync(BaseData.Country, BaseData.CurrentTime, emailMessage).ConfigureAwait(false);
            }
            else
            {
                _ = EmailService.SendEmailAsync(BaseData.Country, BaseData.CurrentTime, emailMessage).ConfigureAwait(false);
            }

            PriceNotificationChecker.InsertRecord(_memberStock.MemberEmail,
                                                  stockFullId,
                                                  StockNotificationType.Buy,
                                                  BaseData.CurrentTime,
                                                  _memberStock.Strategy,
                                                  StockBuyState.Buy);

            if (!_testStatus)
            {
                _ = BaseData.GetLogger().WriteToWorkerLogAsync(BaseData.Country,
                                                               BaseData.CurrentTime,
                                                               "TurtleBuyStrategy",
                                                               $"{_memberStock.Strategy}, first buy ({stockFullId}) email is sent to {_memberStock.MemberEmail}").ConfigureAwait(false);
            }

            if (_isBackTest)
            {
                await BackTestFirstBuyOperationAsync(previousHighBoundPrice.Value).ConfigureAwait(false);
            }
        }

        private async Task AddBuyAsync(CancellationToken token)
        {
            // get MemberBuyStock from database
            IMemberBuyStock memberBuyStock = (await DatabaseOperations.GetMemberBuyStocksAsync(_memberStock.MemberEmail).ConfigureAwait(false))?
                                             .FirstOrDefault(a => (a.State != StockBuyState.Sold || a.State != StockBuyState.Unknown)
                                                                  && string.Equals(a.MemberEmail, _memberStock.MemberEmail, StringComparison.OrdinalIgnoreCase)
                                                                  && a.Country == _memberStock.Country
                                                                  && string.Equals(a.StockId, _memberStock.StockId, StringComparison.OrdinalIgnoreCase));

            if (memberBuyStock == null)
            {
                return;
            }

            string stockFullId = $"{memberBuyStock.Country.GetShortName()}.{memberBuyStock.StockId}";
            decimal buyPrice = memberBuyStock.BuyPrice;
            decimal nValue = memberBuyStock.NValue;
            decimal firstAddPrice = buyPrice + nValue;
            decimal secondAddPrice = firstAddPrice + nValue;
            decimal thirdAddPrie = secondAddPrice + nValue;
            decimal fourthAddPrice = thirdAddPrie + nValue;

            if (!BaseData.CurrentPriceStorage.TryGetItem(memberBuyStock.Country, memberBuyStock.StockId, out ICurrentPrice target)
                || target == null
                || !BaseData.CurrentTime.IsSameDay(target.LastTradeTime))
            {
                return;
            }

            decimal currentPrice = target.CurrentPrice;
            IEmailTemplate emailTemplate = null;
            StockBuyState newBuyState = StockBuyState.Unknown;
            string stockName = await GetStockNameAsync(stockFullId).ConfigureAwait(false);

            if (token.IsCancellationRequested)
            {
                return;
            }

            // TODO : refactor
            if (firstAddPrice <= currentPrice
                && currentPrice < secondAddPrice
                && memberBuyStock.State == StockBuyState.Buy)
            {
                newBuyState = StockBuyState.FirstAdd;
                decimal newStopPrice = firstAddPrice - (nValue * 2);
                await DatabaseOperations.UpdateMemberBuyStockAsync(memberBuyStock.MemberEmail, memberBuyStock.Country, memberBuyStock.StockId, newStopPrice, newBuyState).ConfigureAwait(false);
                emailTemplate = EmailTemplateProvider.GetAddOnBuyEmailTemplate(memberBuyStock.MemberEmail, memberBuyStock.StockId, stockName, newStopPrice, newBuyState);
            }
            else if (secondAddPrice <= currentPrice
                     && currentPrice < thirdAddPrie
                     && memberBuyStock.State.GetStockBuyStateValue() <= 1)
            {
                newBuyState = StockBuyState.SecondAdd;
                decimal newStopPrice = secondAddPrice - (nValue * 2);
                await DatabaseOperations.UpdateMemberBuyStockAsync(memberBuyStock.MemberEmail, memberBuyStock.Country, memberBuyStock.StockId, newStopPrice, newBuyState).ConfigureAwait(false);
                emailTemplate = EmailTemplateProvider.GetAddOnBuyEmailTemplate(memberBuyStock.MemberEmail, memberBuyStock.StockId, stockName, newStopPrice, newBuyState);
            }
            else if (thirdAddPrie <= currentPrice
                     && currentPrice < fourthAddPrice
                     && memberBuyStock.State.GetStockBuyStateValue() <= 2)
            {
                newBuyState = StockBuyState.ThirdAdd;
                decimal newStopPrice = thirdAddPrie - (nValue * 2);
                await DatabaseOperations.UpdateMemberBuyStockAsync(memberBuyStock.MemberEmail, memberBuyStock.Country, memberBuyStock.StockId, newStopPrice, newBuyState).ConfigureAwait(false);
                emailTemplate = EmailTemplateProvider.GetAddOnBuyEmailTemplate(memberBuyStock.MemberEmail, memberBuyStock.StockId, stockName, newStopPrice, newBuyState);
            }
            else if (fourthAddPrice <= currentPrice
                     && memberBuyStock.State.GetStockBuyStateValue() <= 3)
            {
                newBuyState = StockBuyState.FourthAdd;
                decimal newStopPrice = fourthAddPrice - (nValue * 2);
                await DatabaseOperations.UpdateMemberBuyStockAsync(memberBuyStock.MemberEmail, memberBuyStock.Country, memberBuyStock.StockId, newStopPrice, newBuyState).ConfigureAwait(false);
                emailTemplate = EmailTemplateProvider.GetAddOnBuyEmailTemplate(memberBuyStock.MemberEmail, memberBuyStock.StockId, stockName, newStopPrice, newBuyState);
            }

            if (emailTemplate != null
                && PriceNotificationChecker.CanNotify(memberBuyStock.MemberEmail,
                                                      stockFullId,
                                                      StockNotificationType.Buy,
                                                      BaseData.CurrentTime,
                                                      memberBuyStock.Strategy,
                                                      newBuyState))
            {
                if (_testStatus)
                {
                    await EmailService.SendEmailAsync(BaseData.Country, BaseData.CurrentTime, emailTemplate).ConfigureAwait(false);
                }
                else
                {
                    await EmailService.SendEmailAsync(BaseData.Country, BaseData.CurrentTime, emailTemplate).ConfigureAwait(false);
                }

                PriceNotificationChecker.InsertRecord(memberBuyStock.MemberEmail, stockFullId, StockNotificationType.Buy, BaseData.CurrentTime, memberBuyStock.Strategy, newBuyState);

                if (!_testStatus)
                {
                    Task t = BaseData.GetLogger().WriteToWorkerLogAsync(BaseData.Country, BaseData.CurrentTime, "TurtleBuyStrategy", $"{memberBuyStock.Strategy}, add buy ({stockFullId}) email is sent to {emailTemplate.ReceipentEmail} and new state is {newBuyState}");
                }
            }
        }
    }
}
