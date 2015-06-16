using System;
using System.Collections.Concurrent;
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
using SuperSocket.SocketEngine;

namespace RoyNet.GateServer
{
    public class GatewayServer : AppServer<PlayerSession, BinaryRequestInfo>
    {
        private readonly NetMQContext _netMqContext;
        private readonly PullSocket _pullSocket;
        private readonly PushSocket _pushSocket;
        private CancellationTokenSource _cancelToken;
        private Thread _receThread2GameServer;

        public string GameServerAddress { get; private set; }
        public string LoginServerAddress { get; private set; }
        public GatewayServer(): base(new GatewayReceiveFilterFactory()) // 7 parts but 8 separators
        {
            _netMqContext = NetMQContext.Create();
            _pullSocket = _netMqContext.CreatePullSocket();
            _pushSocket = _netMqContext.CreatePushSocket();
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            GameServerAddress = config.Options["gameServerAddress"];
            LoginServerAddress = config.Options["loginServerAddress"];
            return base.Setup(rootConfig, config);
        }

        public override bool Start()
        {
            _pullSocket.Bind(GameServerAddress+"out");
            _pushSocket.Connect(GameServerAddress+"in");

            _receThread2GameServer = new Thread(ReceiveGameServer);
            _cancelToken = new CancellationTokenSource();
            _receThread2GameServer.Start();

            return base.Start();
        }

        void ReceiveGameServer()
        {
            while (!_cancelToken.IsCancellationRequested)
            {
                try
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
                        sessions.AddRange(GetAllSessions().Where(s=>s.IsLogin));
                    }
                    else
                    {
                        for (int i = 0; i < userCount; i++)
                        {
                            long netHandle = converter.ToInt64(data, offset);
                            offset += 8;
                            sessions.AddRange(this.GetSessions(s => s.NetHandle == netHandle));
                        }
                    }
                    var package = new ArraySegment<byte>(data, offset, data.Length - offset);
                    foreach (PlayerSession session in sessions)
                    {
                        session.Send(package);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
        }

        protected override void OnNewSessionConnected(PlayerSession session)
        {
            base.OnNewSessionConnected(session);
        }

        public override void Stop()
        {
            _cancelToken.Cancel();
            base.Stop();
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

        internal void Push2GameServer(PlayerSession session, byte[] data)
        {
            var converter = EndianBitConverter.Big;
            byte[] sendData = new byte[data.Length + 10];
            int offset = 0;
            converter.CopyBytes(session.NetHandle, sendData, 0);
            offset += 8;
            converter.CopyBytes((ushort)data.Length, sendData, offset);
            offset += 2;
            Buffer.BlockCopy(data, 0, sendData, offset, data.Length);
            _pushSocket.Send(sendData);
        }
    }
}
