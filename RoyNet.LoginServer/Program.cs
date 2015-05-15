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
            var ips = GetIP();
            Config = ConfigurationManager.GetSection("loginServer") as LoginServerConfig;
            Debug.Assert(Config != null, "Config != null");

            var uris = ips.Select(ip => new Uri("http://" + ip + ":" + Config.Port)).ToList();
            uris.Add(new Uri("http://127.0.0.1:"+Config.Port));
            Host = new NancyHost(uris.ToArray());
            Host.Start();
            Console.WriteLine("你的服务器已经启动");
            Console.ReadLine();//可以在这里写一个循环，用于运行中的命令行控制。
        }

        protected static IEnumerable<string> GetIP()   //获取本地IP
        {
            List<string> ips = new List<string>();
            System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            for (int i = 0; i != ipEntry.AddressList.Length; i++)
            {
                if (ipEntry.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ips.Add(ipEntry.AddressList[i].ToString());
                }
            }
            return ips;
        }
    }
}
