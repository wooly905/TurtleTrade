using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using TT.StockQuoteSource;
using TT.StockQuoteSource.Contracts;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Config;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction.Storage;
using TurtleTrade.Abstraction.Utilities;
using TurtleTrade.Database;
using TurtleTrade.Infrastructure.DateTimeTools;
using TurtleTrade.Infrastructure.EmailTemplates;
using TurtleTrade.Infrastructure.Storage;

namespace TurtleTrade.Infrastructure
{
    public class BaseData : IBaseData
    {
        private IConfiguration _configuration;
        private readonly IDateTimeTool2 _dateTimeTool;
        private IStockQuoteProvider _stockSourceProvider;
        private readonly ITurtleLogger _logger;
        private IDatabaseOperations _dbOperations;
        private INofiticationService _nofiticationService;
        private IEmailTemplateProvider _emailTemplateProvider;
        private IStockPriceNotificationChecker _stockPriceNotificationChecker;
        private ICurrentPriceStorage _priceStorage;

        public BaseData(CountryKind country, ITurtleLogger logger, ISystemConfig systemConfig, bool runInTestMode = false)
        {
            Country = country;
            _dateTimeTool = DateTimeFactory.GenerateDateTimeTool(country);
            _logger = logger;
            SystemConfig = systemConfig;
            RunInTestMode = runInTestMode;
        }

        public BaseData(CountryKind country, IDateTimeTool2 variableDateTimeTool, ITurtleLogger logger, ISystemConfig systemConfig)
        {
            Country = country;
            _dateTimeTool = variableDateTimeTool;
            _logger = logger;
            SystemConfig = systemConfig;
        }

        public DateTime CurrentTime => _dateTimeTool.GetTime();

        public CountryKind Country { get; }

        public ISystemConfig SystemConfig { get; }

        public bool RunInTestMode { get; }

        public IDatabaseOperations GetDatabaseOperations()
        {
            if (_dbOperations != null)
            {
                return _dbOperations;
            }

            if (RunInTestMode)
            {
                _dbOperations = new MemoryDatabaseOperations();
            }
            else
            {
                _dbOperations = new TurtleDatabaseOperations(SystemConfig.SystemInfo.ProductionTurtleDBConnectionString);
            }

            return _dbOperations;
        }

        public INofiticationService GetNotificationService()
        {
            return _nofiticationService ?? (_nofiticationService = new TurtleEmailService(GetLogger(), SystemConfig.SMTPInfo));
        }

        public IReadOnlyList<IStockQuoteDataSource> GetStockDataSources()
        {
            if (_stockSourceProvider == null)
            {
                _stockSourceProvider = new StockQuoteSourceProvider(GetStockQuoteSourceConfiguration(), Country.ConvertToTTStockQuoteSourceCountry());
            }

            return _stockSourceProvider.GetStockDataSources();
        }

        private IConfiguration GetStockQuoteSourceConfiguration()
        {
            if (_configuration != null)
            {
                return _configuration;
            }

            IConfigurationBuilder configBuilder = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory)
                                                                            .AddJsonFile("StockQuoteSourceConfig.json");
            _configuration = configBuilder.Build();

            return _configuration;
        }

        public IStockPriceNotificationChecker CreateStockPriceNotificationChecker()
        {
            return _stockPriceNotificationChecker ?? (_stockPriceNotificationChecker = new StockPriceNotificationChecker());
        }

        public IEmailTemplateProvider CreateEmailTemplateProvider()
        {
            return _emailTemplateProvider ?? (_emailTemplateProvider = new EmailTemplateProvider());
        }

        public ITurtleLogger GetLogger() => _logger;

        public ICurrentPriceStorage CurrentPriceStorage
        {
            get
            {
                return _priceStorage ?? (_priceStorage = new CurrentPriceStorage());
            }

            set
            {
                _priceStorage = value;
            }
        }
    }
}
