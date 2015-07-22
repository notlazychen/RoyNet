using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using log4net.Repository.Hierarchy;
using MiscUtil.Conversion;
using NetMQ;
using NetMQ.Sockets;
using RoyNet.Util;
using SuperSocket.SocketBase.Logging;

namespace RoyNet.Server.Gate
{
    class MessageQueue
    {
        public MessageQueue(MessageQueueConfig config, NetMQContext mqContext, ILog logger, IDictionary<long, PlayerSession> sessionDIc)
        {
            _sessionDicRef = sessionDIc;
            _logger = logger;
            Port = config.Port;
            Name = config.Name;
            Consumers = config.Customers.Cast<Consumer>().ToList();
            _thread = new TaskThread(Name, OnReceived);
            _pullSocket = mqContext.CreatePullSocket();
            _pushSocket = mqContext.CreatePushSocket();
        }

        private readonly IDictionary<long, PlayerSession> _sessionDicRef;
        private readonly ILog _logger;
        private readonly PullSocket _pullSocket;
        private readonly PushSocket _pushSocket;
        private readonly TaskThread _thread;

        public string Name { get; private set; }
        public int Port { get; private set; }
        public IEnumerable<Consumer> Consumers { get; private set; }

        private void OnReceived()
        {
            try
            {
                if (!_pullSocket.HasIn)
                    return;
                byte[] data = _pullSocket.Receive();
                var converter = EndianBitConverter.Big;
                int offset = 0;
                int userCount = converter.ToInt32(data, offset);
                offset += 4;
                var sessions = new List<PlayerSession>();
                if (userCount == 0)
                {
                    //群发
                    sessions.AddRange(_sessionDicRef.Values);
                }
                else
                {
                    for (int i = 0; i < userCount; i++)
                    {
                        long netHandle = converter.ToInt64(data, offset);
                        offset += 8;
                        sessions.Add(_sessionDicRef[netHandle]);
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
                _logger.Error(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public void Connect()
        {
            foreach (Consumer consumer in Consumers)
            {
                string sendAddress = string.Format("tcp://{0}:{1}", consumer.IP, consumer.Port);
                _pushSocket.Connect(sendAddress);
            }
        }

        public void StartListening()
        {
            string listenAddr = string.Format("tcp://*:{0}", Port);
            _pullSocket.Bind(listenAddr);
            _thread.Start();
        }

        public void Push(PlayerSession session, byte[] data)
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

        public void Close()
        {
            _pullSocket.Close();
            _pushSocket.Close();
            _thread.Stop();
        }
    }
}
