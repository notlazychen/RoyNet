using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace RoyNet
{
    public class Launcher
    {
        public static Launcher Create()
        {
            return new Launcher();
        }

        public List<ServerBase> Servers = new List<ServerBase>();

        public void Ignition()
        {
            var config = ConfigurationManager.GetSection("royNet") as RoyNetConfiguration;
            Debug.Assert(config != null, "royNet Config node is undefined");
            foreach (IServerConfig serverConfig in config.Servers)
            {
                var configSection = new ServerConfig(serverConfig);
                var type = config.ServerTypes[configSection.ServerTypeName].TypeProvider;

                AppDomain newDomain = AppDomain.CreateDomain(configSection.Name);
                var assembly = newDomain.Load(type.AssemblyName);
                var obj = newDomain.CreateInstanceAndUnwrap(type.AssemblyName, type.ClassFullName,
                        true,
                        BindingFlags.CreateInstance,
                        null,
                        null,
                        null,
                        new object[0]);
                var instance = obj as ServerBase;
                if (instance != null)
                {
                    instance.Configure(configSection);
                    Servers.Add(instance);
                }
            }
            //初始化完成
            //开始启动
            foreach (ServerBase serverBase in Servers)
            {
                serverBase.Startup();
            }
        }

        public void Stop()
        {
            foreach (ServerBase serverBase in Servers)
            {
                serverBase.Stop();
            }
        }
    }
}
