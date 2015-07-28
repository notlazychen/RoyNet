using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;

namespace RoyNet.Server.Gate
{
    public class GatewayServer: ServerBase
    {
        public GatewayAppServer AppServer { get; private set; }
        protected override void OnConfigure(IServerConfig config)
        {
            AppServer = new GatewayAppServer();
            int port = int.Parse(config.Options["port"]);
            var serverConfig = new SuperSocket.SocketBase.Config.ServerConfig()
            {
                Ip = "Any",
                Port = port,
                KeepAliveTime = 10,
                DisableSessionSnapshot = true,
                SendTimeOut = 0,
                MaxConnectionNumber = 3000
            };
            serverConfig.OptionElements = config.OptionElements;
            serverConfig.Options = config.Options;

            if (!AppServer.Setup(new RootConfig(), serverConfig)) 
            {
                throw new Exception("setup failed");
            }
        }

        protected override void OnStart()
        {
            if (!AppServer.Start())
            {
                throw new Exception("start failed");
            }
        }

        protected override void OnStop()
        {
            AppServer.Stop();
        }

        protected override void OnRequire()
        {

        }
    }
}
