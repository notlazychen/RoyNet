using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiscUtil.Conversion;
using NetMQ;
using NetMQ.Sockets;
using NetMQ.zmq;
using RoyNet.Engine;
using RoyNet.GameServer;
using RoyNet.GameServer.Entity;

namespace RoyNet.GameServer
{
    class Server : IDisposable
    {
        private readonly NetMQContext _netMqContext;
        private readonly PullSocket _pullSocket;
        private readonly PushSocket _pushSocket;
        private readonly TaskThread _mainThread;
        private readonly TaskThread _sendThread;
        private readonly TaskThread _receThread;
        private readonly ConcurrentQueue<Action> _actionsWaiting = new ConcurrentQueue<Action>();
        private readonly ConcurrentQueue<IMessageEntity> _msgsWaiting = new ConcurrentQueue<IMessageEntity>();
        private readonly Dictionary<string, RequestFactor> _commands = new Dictionary<string, RequestFactor>();
        public string Address { get; private set; }
        public bool IsRunning { get; private set; }

        public Server(string address)
        {
            _mainThread = new TaskThread("执行", MainLoop);
            _sendThread = new TaskThread("发送", Send);
            _receThread = new TaskThread("接收", Receive);
            Address = address;
            _netMqContext = NetMQContext.Create();
            _pullSocket = _netMqContext.CreatePullSocket();
            _pushSocket = _netMqContext.CreatePushSocket();
        }
        
        public void Open()
        {
            MethodInfo methodSerializer = typeof(ProtoBuf.Serializer).GetMethod("Deserialize");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                IEnumerable<Type> ts = assembly.GetExportedTypes();
                foreach (Type t in ts)
                {
                    if (t.IsAbstract) continue;
                    if (t.IsSubclassOf(typeof(CommandBase)))
                    {
                        var ins = Activator.CreateInstance(t) as CommandBase;
                        Debug.Assert(t.BaseType != null, "t.BaseType != null");
                        var entityType = t.BaseType.GetGenericArguments()[0];
                        Debug.Assert(ins != null, "ins != null");
                        _commands[ins.Name] = new RequestFactor()
                        {
                            Command = ins, 
                            PackageType = entityType,
                            CreatePackageMethod = methodSerializer.MakeGenericMethod(entityType)
                        };
                    }
                }
            }

            _pullSocket.Bind(Address + "in");
            _pushSocket.Connect(Address + "out");
            _mainThread.Start();
            _sendThread.Start();
            _receThread.Start();
            IsRunning = true;
        }

        void Send()
        {
        }

        void Receive()
        {
            if (!_pullSocket.HasIn)
            {
                return;
            }
            byte[] data = _pullSocket.Receive();
            int offset = 0;
            var converter = EndianBitConverter.Big;
            int userID = converter.ToInt32(data, offset);
            offset += 4;
            int length = converter.ToUInt16(data, offset);
            offset += 2;
            int cmdName = converter.ToInt32(data, offset);
            offset += 4;
            RequestFactor factor;
            if (_commands.TryGetValue(cmdName.ToString("D"), out factor))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(data, offset, length - 4);
                    stream.Position = 0;
                    var package = factor.CreatePackageMethod.Invoke(null, new object[] { stream });
                    _actionsWaiting.Enqueue(() =>
                    {
                        factor.Command.Execute(null, package);
                    });
                }
            }
            else
            {
                //todo:不认识的协议
            }
        }

        //游戏主循环
        void MainLoop()
        {
            //todo: 超时判断
            Action act;
            while (_actionsWaiting.TryDequeue(out act))
            {
                act.Invoke();
            }
        }
        
        public void Close()
        {
            Dispose();
            IsRunning = false;
        }

        public void Dispose()
        {
            if (!IsRunning)
                return;

            _mainThread.Stop();
            _sendThread.Stop();
            _receThread.Stop();

            _pullSocket.Unbind(Address + "in");
            _pullSocket.Dispose();
            _pushSocket.Disconnect(Address + "out");
            _pushSocket.Dispose();
            _netMqContext.Dispose();
            IsRunning = false;
        }
    }

    public class RequestFactor
    {
        public CommandBase Command;
        public Type PackageType;
        public MethodInfo CreatePackageMethod;

        //public object Serializer(MemoryStream stream)
        //{
        //    MethodInfo func = createMethod.MakeGenericMethod(PackageType);
        //    return func.Invoke(null, new object[] { stream });
        //}
    }
}
