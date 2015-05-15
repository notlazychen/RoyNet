using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;

namespace RoyNet.LoginServer
{
    class Program
    {
        public static LoginServerConfig Config { get; private set; }
        public static NancyHost Host { get; private set; }
        static void Main(string[] args)
        {
            Config = ConfigurationManager.GetSection("loginServer") as LoginServerConfig;
            Debug.Assert(Config != null, "Config != null");

            string url = Config.HostUrl;
            Host = new NancyHost(new Uri(url));
            Host.Start();
            Console.WriteLine("你的服务器已经启动，访问地址是 {0}", url);
            Console.ReadLine();//可以在这里写一个循环，用于运行中的命令行控制。
        }
    }
}
