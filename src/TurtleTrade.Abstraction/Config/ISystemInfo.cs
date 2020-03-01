namespace TurtleTrade.Abstraction.Config
{
    public interface ISystemInfo
    {
        string AdminEmail { get; set; }

        string ProductionTurtleDBConnectionString { get; set; }

        string TestTurtleDBConnectionString { get; set; }
    }
}
