#region using

using System;
using System.IO;
using System.Reflection;

#endregion

namespace ServerHolder
{
    internal class ServiceInfo
    {
        public ServiceInfo(dynamic config, dynamic serverConfig)
        {
            Config = config;
            ServerConfig = serverConfig;

            if (Config.Type == "dll")
            {
                var file = Path.Combine(Environment.CurrentDirectory, config.DLLName);
                Assembly ass = Assembly.LoadFile(file);
                ServiceType = ass.GetType(config.EntryType);
            }
            else if (Config.Type == "exe")
            {
            }
        }

        public dynamic Config { get; private set; }

        public string Name
        {
            get { return Config.ServiceName; }
        }

        public dynamic ServerConfig { get; private set; }
        public Type ServiceType { get; private set; }
    }
}