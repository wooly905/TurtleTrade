using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Utilities;

namespace TurtleTrade.Infrastructure.Logger
{
    public class FileLogger : ITurtleLogger, IDisposable
    {
        private const string LogFolder = "LogFolders";
        private readonly string _logFolderPath;
        private bool _canWriteLog;
        private readonly SemaphoreSlim _workerLogLock;

        public FileLogger()
        {
            _workerLogLock = new SemaphoreSlim(1);

            // LOG settings
            _logFolderPath = Path.Combine(Environment.CurrentDirectory, LogFolder);
            MakeSureLogFolderExist();
        }

        public async Task WriteToWorkerLogAsync(CountryKind country, DateTime time, string workerKind, string message)
        {
            if (string.IsNullOrEmpty(message)
                || string.IsNullOrEmpty(workerKind)
                || !_canWriteLog)
            {
                return;
            }

            string fileName = $"{country.GetShortName()}-TT2WorkerLog-{time:yyyyMMdd}.csv";
            string filePath = Path.Combine(_logFolderPath, fileName);
            string data = $"{time:HH:mm:ss},{workerKind},{message}";

            await WriteContentToLogAsync(data, filePath).ConfigureAwait(false);
        }

        private void MakeSureLogFolderExist()
        {
            try
            {
                if (!Directory.Exists(_logFolderPath))
                {
                    Directory.CreateDirectory(_logFolderPath);
                }

                _canWriteLog = true;
            }
            catch
            { }
        }

        public async Task WriteToHeartBeatLogAsync(CountryKind country, DateTime time, string workerKind)
        {
            if (string.IsNullOrEmpty(workerKind) || !_canWriteLog)
            {
                return;
            }

            string fileName = $"{country.GetShortName()}-TT2HeartBeat-{time:yyyyMMdd}.csv";
            string filePath = Path.Combine(_logFolderPath, fileName);
            string data = $"{time:HH:mm:ss},{workerKind } sent a heart beat";

            await WriteContentToLogAsync(data, filePath).ConfigureAwait(false);
        }

        public async Task WriteToErrorLogAsync(CountryKind country, DateTime time, string workerKind, Exception ex)
        {
            if (ex == null
                || string.IsNullOrEmpty(workerKind)
                || !_canWriteLog)
            {
                return;
            }

            string fileName = $"{country.GetShortName()}-TT2ErrorLog-{time:yyyyMMdd}.csv";
            string filePath = Path.Combine(_logFolderPath, fileName);
            StringBuilder errorStringBuilder = new StringBuilder();
            errorStringBuilder.Append("Exception message = ").AppendLine(ex.Message);
            errorStringBuilder.Append("Exception stack trace = ").AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                errorStringBuilder.Append("  Inner exception message = ").AppendLine(ex.InnerException.Message);
                errorStringBuilder.Append("  Inner exception stack trace = ").AppendLine(ex.InnerException.StackTrace);
            }

            string data = $"{time:HH:mm:ss},{workerKind}, {errorStringBuilder}";

            await WriteContentToLogAsync(data, filePath).ConfigureAwait(false);
        }

        public async Task WriteToEmailLogAsync(CountryKind country, DateTime time, string workerKind, string emailContent)
        {
            if (string.IsNullOrEmpty(emailContent)
                || string.IsNullOrEmpty(workerKind)
                || !_canWriteLog)
            {
                return;
            }

            string fileName = $"{country.GetShortName()}-TT2EmailLog-{time:yyyyMMdd}.csv";
            string filePath = Path.Combine(_logFolderPath, fileName);
            string data = $"{time:HH:mm:ss},{workerKind} sent an email. Content = {emailContent}";

            await WriteContentToLogAsync(data, filePath).ConfigureAwait(false);
        }

        public async Task WriteToCurrentPriceLogAsync(CountryKind country, string data)
        {
            if (string.IsNullOrEmpty(data)
                || country == CountryKind.Test
                || country == CountryKind.Test2
                || country == CountryKind.Unknown
                || !_canWriteLog)
            {
                return;
            }

            string fileName = $"{country.GetShortName()}-CurrentPrice.csv";
            string filePath = Path.Combine(_logFolderPath, fileName);

            await WriteContentToLogAsync(data, filePath, false).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private async Task WriteContentToLogAsync(string data, string filePath, bool fileAppend = true)
        {
            await _workerLogLock.WaitAsync().ConfigureAwait(false);

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, fileAppend))
                {
                    await sw.WriteLineAsync(data).ConfigureAwait(false);
                }
            }
            finally
            {
                _workerLogLock.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                _workerLogLock?.Dispose();
            }

            // Free native resources
        }
    }
}
