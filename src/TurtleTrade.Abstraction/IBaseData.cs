using System;
using System.Collections.Generic;
using TT.StockQuoteSource.Contracts;
using TurtleTrade.Abstraction.Config;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction.Storage;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Abstraction
{
    public interface IBaseData
    {
        CountryKind Country { get; }

        ICurrentPriceStorage CurrentPriceStorage { get; set; }

        DateTime CurrentTime { get; }

        bool RunInTestMode { get; }

        ISystemConfig SystemConfig { get; }

        IEmailTemplateProvider CreateEmailTemplateProvider();

        IStockPriceNotificationChecker CreateStockPriceNotificationChecker();

        IDatabaseOperations GetDatabaseOperations();

        ITurtleLogger GetLogger();

        INofiticationService GetNotificationService();

        IReadOnlyList<IStockQuoteDataSource> GetStockDataSources();
    }
}
