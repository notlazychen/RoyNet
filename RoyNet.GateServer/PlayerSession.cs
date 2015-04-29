using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Protocol;

namespace RoyNet.GateServer
{
    public class PlayerSession : AppSession<PlayerSession, BinaryRequestInfo>
    {
        private static long _netHandleAutoIncrease = 0;

        public bool IsLogin
        {
            get { return _isLogin; }
        }

        private long _netHandle;
        private bool _isLogin;

        public long NetHandle
        {
            get { return _netHandle; }
        }

        public GatewayServer Server { get; private set; }

        protected override void OnSessionStarted()
        {
            Server = AppServer as GatewayServer;
            base.OnSessionStarted();
        }

        public void Login()
        {
            _isLogin = true;
            _netHandle = Interlocked.Increment(ref _netHandleAutoIncrease);
        }
    }
}
