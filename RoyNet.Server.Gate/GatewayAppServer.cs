using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using MiscUtil.Conversion;
using NetMQ;
using ProtoBuf;
using RoyNet.Server.Gate.Entity;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace RoyNet.Server.Gate
{
    public class GatewayAppServer : AppServer<PlayerSession, BinaryRequestInfo>
    {
        private readonly NetMQContext _netMqContext;

        private List<MessageQueue> _messageQueues = new List<MessageQueue>();
        public readonly ConcurrentDictionary<long, PlayerSession> ManagedSessionByNetHandle = new ConcurrentDictionary<long, PlayerSession>();
        
        public string LoginServerUrl { get; private set; }
        public GatewayAppServer(): base(new GatewayReceiveFilterFactory()) // 7 parts but 8 separators
        {
            _netMqContext = NetMQContext.Create();
        }

        protected override bool Setup(IRootConfig rootConfig, SuperSocket.SocketBase.Config.IServerConfig config)
        {
            LoginServerUrl = config.Options["loginServerUrl"];
            var mqs = config.GetChildConfig<ConfigurationElementCollection<MessageQueueConfig>>("messageQueues");

            foreach (MessageQueueConfig mqConfig in mqs)
            {
                _messageQueues.Add(new MessageQueue(mqConfig, _netMqContext, Logger, ManagedSessionByNetHandle));
            }

            foreach (MessageQueue mq in _messageQueues)
            {
                mq.StartListening();
            }

            return base.Setup(rootConfig, config);
        }
        
        public override bool Start()
        {
            foreach (MessageQueue mq in _messageQueues)
            {
                mq.Connect();
            }
            return base.Start();
        }

        protected override void OnNewSessionConnected(PlayerSession session)
        {
            base.OnNewSessionConnected(session);
        }

        public override void Stop()
        {
            //_cancelToken.Cancel();
            base.Stop();
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            foreach (MessageQueue mq in _messageQueues)
            {
                mq.Close();
            }
            _netMqContext.Dispose();
        }

        protected override void OnSessionClosed(PlayerSession session, CloseReason reason)
        {
            base.OnSessionClosed(session, reason);

            var package = new G2G_ToGameDisconnect()
            {
                Reason = reason.ToString()
            };

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, package);
                var packageData = ms.ToArray();

                var converter = EndianBitConverter.Big;
                byte[] sendData = new byte[packageData.Length + 4];
                converter.CopyBytes((int)CMD_G2G.ToGameDisconnect, sendData, 0);
                Buffer.BlockCopy(packageData, 0, sendData, 4, packageData.Length);
                
                foreach (MessageQueue mq in _messageQueues)
                {
                    mq.Push(session, sendData);
                }
            }

            PlayerSession savedSession;
            ManagedSessionByNetHandle.TryRemove(session.NetHandle, out savedSession);
        }

        public void Push2GameServer(PlayerSession session, byte[] data)
        {
            foreach (MessageQueue mq in _messageQueues)
            {
                mq.Push(session, data);
            }
        }
    }
}
