using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TurtleTrade.Abstraction.ServiceWorkers;

namespace TurtleTrade.ServiceWorkers
{
    public class ServiceManager : IDisposable
    {
        private IList<IServiceWorker> _workers;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;

        public ServiceManager()
        {
            _workers = new List<IServiceWorker>();
        }

        public void AddWorker(IServiceWorker worker)
        {
            if (worker != null && !_isRunning)
            {
                _workers.Add(worker);
            }
        }

        public void RemoveWorkers()
        {
            if (_isRunning)
            {
                return;
            }

            if (_workers.Count > 0)
            {
                _workers.Clear();
            }
        }

        public bool IsRunning => _isRunning;

        public void StartWokers()
        {
            if (_isRunning || _workers.Count == 0)
            {
                return;
            }

            if (_cancellationTokenSource?.IsCancellationRequested == false)
            {
                _cancellationTokenSource.Cancel(false);
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            Task.Factory.StartNew(async () =>
            {
                _isRunning = true;

                while (true)
                {
                    foreach (var worker in _workers)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        if (worker.State != ServiceWorkerState.Running)
                        {
                            _ = Task.Run(() => worker.RunAsync(_cancellationToken));
                        }
                    }

                    // wait 60 seconds by default
                    for (int i = 0; i < 60; i++)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                }
            }, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void StopWokers()
        {
            if (_workers.Count == 0
                || !_isRunning
                || _cancellationTokenSource == null)
            {
                return;
            }

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel(false);
            }

            _isRunning = false;
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
                if (_cancellationTokenSource != null)
                {
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Cancel(false);
                    }

                    _cancellationTokenSource.Dispose();
                }

                if (_workers.Count > 0)
                {
                    _workers.Clear();
                }
            }
        }
    }
}
