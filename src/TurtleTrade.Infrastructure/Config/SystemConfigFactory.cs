using System;
using System.IO;
using Newtonsoft.Json;
using TurtleTrade.Abstraction.Config;

namespace TurtleTrade.Infrastructure.Config
{
    public static class SystemConfigFactory
    {
        private static readonly Lazy<ISystemConfig> _configSingleton;
        private const string _systemConfigFile = "systemconfig.json";

        static SystemConfigFactory() => _configSingleton = new Lazy<ISystemConfig>(GetConfigInternal, true);

        public static ISystemConfig GetConfig() => _configSingleton.Value;

        private static ISystemConfig GetConfigInternal()
        {
            try
            {
                string json = File.ReadAllText(_systemConfigFile);
                return JsonConvert.DeserializeObject<SystemConfig>(json);
            }
            catch
            {
                // TODO : log ?
            }

            return null;
        }
    }
}
