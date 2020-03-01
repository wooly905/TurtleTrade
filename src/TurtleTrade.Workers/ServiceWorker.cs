using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Config;
using TurtleTrade.Abstraction.ServiceWorkers;
using TurtleTrade.Abstraction.Utilities;
using TurtleTrade.Infrastructure;

[assembly: InternalsVisibleTo("Turtle2.TestProject")]
namespace TurtleTrade.ServiceWorkers
{
    public abstract class ServiceWorker : IServiceWorker
    {
        private DateTime _workerStartTime;
        private DateTime _workerStopTime;
        private static INofiticationService _notificationService;
        private DateTime _testTime;

        protected ServiceWorker(IBaseData baseData)
        {
            State = ServiceWorkerState.Initializing;
            BaseData = baseData;
            SystemConfig = baseData.SystemConfig;
            Country = baseData.Country;
            _notificationService = baseData.GetNotificationService();

            TestStatus = baseData.RunInTestMode;
            _testTime = DateTime.Now;

            State = ServiceWorkerState.Initialized;
        }

        protected IBaseData BaseData { get; }
        protected ISystemConfig SystemConfig { get; }
        public CountryKind Country { get; }
        protected DateTime CurrentTime => TestStatus ? _testTime : BaseData.CurrentTime;

        /// <summary>
        /// Sets current time [For test purpose only]
        /// </summary>
        /// <param name="time"></param>
        internal void SetTestCurrentTime(DateTime time)
        {
            TestStatus = true;
            _testTime = time;
        }

        protected INofiticationService EmailService => _notificationService;

        public string Name { get; protected set; }

        public ServiceWorkerState State { get; protected set; }

        protected bool TestStatus { get; private set; }

        protected void SetWorkerStartTimeEndTime(ServiceWorkerKind kind)
        {
            _workerStartTime = BaseData.GetWorkerStartTime(kind);
            _workerStopTime = BaseData.GetWorkerEndTime(kind);
        }

        public ServiceWorkerKind Kind { get; protected set; }

        protected abstract Task RunInternalAsync(CancellationToken token);

        public async Task RunAsync(CancellationToken token)
        {
            try
            {
                if (CanRunWorker())
                {
                    State = ServiceWorkerState.Running;
                    await RunInternalAsync(token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await BaseData.GetLogger().WriteToErrorLogAsync(Country, CurrentTime, Kind.ToString(), ex).ConfigureAwait(false);
            }

            State = ServiceWorkerState.Stopped;
        }

        private bool CanRunWorker()
        {
            // handle unit test first
            if (TestStatus
                || Country == CountryKind.Unknown
                || Country == CountryKind.Test
                || Country == CountryKind.Test2
                || Kind == ServiceWorkerKind.HistoricalPriceWorker)
            {
                return true;
            }

            // skip weekends
            if (CurrentTime.DayOfWeek == DayOfWeek.Saturday || CurrentTime.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            DateTime workerRunStartTime = new DateTime(CurrentTime.Year, CurrentTime.Month, CurrentTime.Day, _workerStartTime.Hour, _workerStartTime.Minute, 0);
            DateTime workerRunEndTime = new DateTime(CurrentTime.Year, CurrentTime.Month, CurrentTime.Day, _workerStopTime.Hour, _workerStopTime.Minute, 59);

            return BaseData.IsTimeInBetween(workerRunStartTime, workerRunEndTime, CurrentTime);
        }

        protected void WriteToWorkerLog(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            BaseData.GetLogger().WriteToWorkerLogAsync(Country, CurrentTime, Kind.ToString(), message);
        }

        protected void WriteToHeartBeatLog()
        {
            BaseData.GetLogger().WriteToHeartBeatLogAsync(Country, CurrentTime, Kind.ToString());
        }

        protected void WriteToErrorLog(Exception exception)
        {
            if (exception == null)
            {
                return;
            }
             
           BaseData.GetLogger().WriteToErrorLogAsync(Country, CurrentTime, Kind.ToString(), exception);
        }

        protected void WriteToEmailLog(string emailContent)
        {
            if (string.IsNullOrEmpty(emailContent))
            {
                return;
            }

            BaseData.GetLogger().WriteToEmailLogAsync(Country, CurrentTime, Kind.ToString(), emailContent);
        }
    }
}
