using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;

namespace RoyNet.Server.Login
{
    class LoginServer:ServerBase
    {
        public static IEnumerable<string> GetIP()   //获取本地IP
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

        public LoginServerConfig Config { get; private set; }
        public NancyHost Host { get; private set; }


        protected override void OnConfigure(IServerConfig config)
        {
            Config = new LoginServerConfig(config);
            Debug.Assert(Config != null, "Config != null");
        }

        protected override void OnStart()
        {
            var ips = GetIP();
            var uris = ips.Select(ip => new Uri("http://" + ip + ":" + Config.Port)).ToList();
            uris.Add(new Uri("http://127.0.0.1:" + Config.Port));
            HostConfiguration hostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };
            Host = new NancyHost(hostConfigs, uris.ToArray());
            Host.Start();
        }

        protected override void OnStop()
        {
            Host.Stop();
        }

        protected override void OnRequire()
        {
        }
    }
}
