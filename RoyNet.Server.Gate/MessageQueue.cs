using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MiscUtil.Conversion;
using RoyNet.Util;
using SuperSocket.SocketBase.Logging;

namespace RoyNet.Server.Gate
{
    class MessageQueue:IDisposable
    {
        private const int DefaultBufferSize = 102400;
        public MessageQueue(MessageQueueConfig config, ILog logger, IDictionary<long, PlayerSession> sessionDIc)
        {
            _sessionDicRef = sessionDIc;
            _logger = logger;
            Port = config.Port;
            Name = config.Name;
            AllowedIPs = config.AllowedIPArray.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries).ToList();
            _listener = new TcpListener(IPAddress.Any, Port); 
            _thread = new TaskThread(Name, OnReceived);
            _buffer = new byte[DefaultBufferSize];
        }

        private IDictionary<long, PlayerSession> _sessionDicRef { get; set; }
        private readonly ILog _logger;
        private readonly TcpListener _listener;
        private Socket _socket;
        private TaskThread _thread;

        public string Name { get; private set; }
        public int Port { get; private set; }
        private IList<string> AllowedIPs{ get; set; }

        private readonly byte[] _buffer;
        private int _offset = 0;

        private void OnReceived()
        {
            try
            {
                if (_socket == null)
                {
                    var socket = _listener.AcceptSocket();
                    var address = socket.RemoteEndPoint as IPEndPoint;
                    if (socket.AddressFamily == AddressFamily.InterNetwork
                        && address != null
                        && (AllowedIPs.Count == 0 || AllowedIPs.Any(ip => ip == address.Address.ToString())))
                    {
                        _socket = socket;
                    }
                    else
                    {
                        _logger.WarnFormat("一个不允许的IP[{0}]尝试作为游戏服连接网关", address);
                        return;
                    }
                }

                int size = _socket.Receive(_buffer, _offset, _buffer.Length - _offset, SocketFlags.Partial) + _offset;
                _offset = 0;
                //拆包
                var converter = EndianBitConverter.Big;
                while (true)
                {
                    int leftLength = size - _offset;
                    if (leftLength < 4 || leftLength < converter.ToInt32(_buffer, _offset))
                    {
                        //不够了
                        Buffer.BlockCopy(_buffer, _offset, _buffer, 0, leftLength);
                        _offset = leftLength;
                        break;
                    }
                    _offset += 4;
                    int userCount = converter.ToInt32(_buffer, _offset);
                    _offset += 4;
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
                            long netHandle = converter.ToInt64(_buffer, _offset);
                            _offset += 8;
                            PlayerSession session;
                            if (_sessionDicRef.TryGetValue(netHandle, out session))
                            {
                                sessions.Add(session);
                            }
                        }
                    }

                    int packetSize = converter.ToInt16(_buffer, _offset);
                    var package = new ArraySegment<byte>(_buffer, _offset, packetSize + 2);
                    _offset += (2 + packetSize);

                    foreach (PlayerSession session in sessions)
                    {
                        session.Send(package);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + Environment.NewLine + ex.StackTrace);
                if (_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }
            }
        }

        //private void BeginWaitForGameServer()
        //{
        //    _listener.BeginAcceptSocket((p) =>
        //    {
        //        var socket = _listener.EndAcceptSocket(p);
        //        var address = socket.RemoteEndPoint as IPEndPoint;
        //        if (socket.AddressFamily == AddressFamily.InterNetwork && address != null &&
        //            AllowedIPs.Any(ip => ip == address.Address.ToString()))
        //        {
        //            _socket = socket;
        //            _thread = new TaskThread(Name, OnReceived);
        //            _thread.Start();
        //        }
        //        else
        //        {
        //            BeginWaitForGameServer();
        //        }
        //    }, null);
        //}

        public void StartListening()
        {
            _listener.Start();
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

            if(_socket != null)
                _socket.Send(sendData);
        }

        public void Close()
        {
            _thread.Stop();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
