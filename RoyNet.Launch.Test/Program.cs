using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using RoyNet.Server.Gate;

namespace RoyNet.Launch.Test
{
    /// <summary>
    /// 此启动器引用了所有解决方案中依赖的dll，可用于VS调试
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Launcher launcher = Launcher.Create();
            launcher.Ignition();
            Console.WriteLine("All servers have been started!");
            while (true)
            {
                string cmd = Console.ReadLine();
                if (cmd == "stop")
                {
                    launcher.Stop();
                    break;
                }
            }
            Console.WriteLine("byebye");
            //List<ServerBase> Servers = new List<ServerBase>();
            //var config = ConfigurationManager.GetSection("royNet") as RoyNetConfiguration;
            //Debug.Assert(config != null, "royNet Config node is undefined");
            //foreach (IServerConfig serverConfig in config.Servers)
            //{
            //    var configSection = new ServerConfig(serverConfig);
            //    var type = config.ServerTypes[configSection.ServerTypeName].TypeProvider;

            //    var instance = new GatewayServer();
            //    if (instance != null)
            //    {
            //        instance.Configure(configSection);
            //        Servers.Add(instance);
            //    }
            //    break;
            //}
            ////初始化完成
            ////开始启动
            //foreach (ServerBase serverBase in Servers)
            //{
            //    serverBase.Startup();
            //}

            //Console.ReadLine();
        }
    }
}
