using TurtleTrade.Abstraction.Config;

namespace TurtleTrade.Infrastructure.Config
{
    public class SystemInfo : ISystemInfo
    {
        public SystemInfo(string adminEmail, string productionTurtleDBConnectionString, string testTurtleDBConnectionString)
        {
            AdminEmail = adminEmail;
            ProductionTurtleDBConnectionString = productionTurtleDBConnectionString;
            TestTurtleDBConnectionString = testTurtleDBConnectionString;
        }

        public string AdminEmail { get; set; }

        public string ProductionTurtleDBConnectionString { get; set; }

        public string TestTurtleDBConnectionString { get; set; }
    }
}
