using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace RoyNet.GateServer
{
    public class GatewayServer : AppServer<PlayerSession, BinaryRequestInfo>
    {
        private readonly NetMQContext _netMqContext;
        private readonly PullSocket _pullSocket;
        private readonly PushSocket _pushSocket;
        public string GameServerAddress { get; private set; }
        public GatewayServer(): base(new GatewayReceiveFilterFactory()) // 7 parts but 8 separators
        {
            _netMqContext = NetMQContext.Create();
            _pullSocket = _netMqContext.CreatePullSocket();
            _pushSocket = _netMqContext.CreatePushSocket();
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            GameServerAddress = "ipc://game1";
            return base.Setup(rootConfig, config);
        }

        public override bool Start()
        {
            _pullSocket.Connect(GameServerAddress+"out");
            _pushSocket.Connect(GameServerAddress+"in");
            return base.Start();
        }

        protected override void OnNewSessionConnected(PlayerSession session)
        {
            base.OnNewSessionConnected(session);
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            _pullSocket.Close();
            _pullSocket.Dispose();
            _pushSocket.Close();
            _pushSocket.Dispose();
            _netMqContext.Dispose();
        }

        public void Push(byte[] data)
        {
            _pushSocket.Send(data);
        }

        public byte[] Pull()
        {
            return _pullSocket.Receive();
        }
    }
}
