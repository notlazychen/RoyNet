using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiscUtil.Conversion;
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
        private CancellationTokenSource _cancelToken;
        private Thread _receThread;
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
            _pullSocket.Bind(GameServerAddress+"out");
            _pushSocket.Connect(GameServerAddress+"in");
            _receThread = new Thread(ReceiveGameServer);
            _cancelToken = new CancellationTokenSource();
            _receThread.Start();
            return base.Start();
        }

        void ReceiveGameServer()
        {
            while (!_cancelToken.IsCancellationRequested)
            {
                Thread.Sleep(1);
                if (!_pullSocket.HasIn)
                    continue;
                byte[] data = _pullSocket.Receive();
                var converter = EndianBitConverter.Big;
                int offset = 0;
                int userCount = converter.ToInt32(data, offset);
                offset += 4;
                var sessions = new List<PlayerSession>();
                if (userCount == 0)
                {
                    //群发
                    sessions.AddRange(GetAllSessions());
                }
                else
                {
                    for (int i = 0; i < userCount; i++)
                    {
                        int userID = converter.ToInt32(data, offset);
                        offset += 4;
                        sessions.AddRange(this.GetSessions(s => s.UserID == userID));
                    }
                }
                var package = new ArraySegment<byte>(data, offset, data.Length - offset);
                foreach (PlayerSession session in sessions)
                {
                    session.Send(package);
                }
            }
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
    }
}
