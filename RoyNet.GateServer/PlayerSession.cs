using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Protocol;

namespace RoyNet.GateServer
{
    public class PlayerSession : AppSession<PlayerSession, BinaryRequestInfo>
    {
        public bool IsLogin { get; set; }
        public int UserID { get; set; }
        public GatewayServer Server { get; private set; }

        protected override void OnSessionStarted()
        {
            Server = AppServer as GatewayServer;
            base.OnSessionStarted();

        }
    }
}
