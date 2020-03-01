namespace TurtleTrade.Abstraction.ServiceWorkers
{
    public enum ServiceWorkerState
    {
        Unknown = -1,
        Initializing = 0,
        Initialized = 1,
        Stopped = 2,
        Running = 3
    }
}
