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
using RoyNet.GameServer;
using RoyNet.GameServer.Logic.Login;
using RoyNet.GateServer.Entity;

namespace RoyNet.GameServer
{
    public class Server : IDisposable
    {
        public static Server Current { get; private set; }
        private readonly NetMQContext _netMqContext;
        private readonly PullSocket _pullSocket;
        private readonly PushSocket _pushSocket;
        private readonly TaskThread _mainThread;
        private readonly TaskThread _sendThread;
        private readonly TaskThread _receThread;
        private readonly ConcurrentQueue<Tuple<long,CommandBase, object>> _actionsWaiting = new ConcurrentQueue<Tuple<long, CommandBase, object>>();
        private readonly ConcurrentQueue<IMessageEntity> _msgsWaiting = new ConcurrentQueue<IMessageEntity>();
        private readonly Dictionary<string, RequestFactor> _commands = new Dictionary<string, RequestFactor>();
        public string Address { get; private set; }
        public bool IsRunning { get; private set; }

        private readonly Dictionary<long, Player> _allPlayers = new Dictionary<long, Player>();
        public NLogLoggingService Logger { get; private set; }

        private readonly List<RoyNetModule> _modules = new List<RoyNetModule>();

        public Server(string address)
        {
            _mainThread = new TaskThread("执行", MainLoop);
            _sendThread = new TaskThread("发送", OnSend);
            _receThread = new TaskThread("接收", OnReceive);
            Address = address;
            _netMqContext = NetMQContext.Create();
            _pullSocket = _netMqContext.CreatePullSocket();
            _pushSocket = _netMqContext.CreatePushSocket();
            Current = this;
        }
        
        public void Open()
        {
            Logger = new NLogLoggingService("roynet");
            Logger.Info("game server is starting");

            List<CommandBase> commands = new List<CommandBase>()
            {
                new LoginCommand()
            };


            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                IEnumerable<Type> ts = assembly.GetExportedTypes();
                foreach (Type t in ts)
                {
                    if (t.IsAbstract) continue;
                    if (t.IsSubclassOf(typeof (RoyNetBootstrapper)))
                    {
                        var ins = Activator.CreateInstance(t) as RoyNetBootstrapper;
                        var modules = new List<RoyNetModule>();
                        ins.ServerStartup(modules);
                        foreach (RoyNetModule module in modules)
                        {
                            module.Startup(commands);
                        }
                    }
                }
            }


            MethodInfo methodSerializer = typeof(ProtoBuf.Serializer).GetMethod("Deserialize");
            foreach (CommandBase command in commands)
            {
                Type t = command.GetType();
                Debug.Assert(t.BaseType != null, "t.BaseType != null");
                var entityType = t.BaseType.GetGenericArguments()[0];
                Debug.Assert(command != null, "ins != null");
                _commands[command.Name] = new RequestFactor()
                {
                    Command = command,
                    PackageType = entityType,
                    CreatePackageMethod = methodSerializer.MakeGenericMethod(entityType)
                };
                Logger.Info("load command:{0}", command.Name);
            }

            //foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    IEnumerable<Type> ts = assembly.GetExportedTypes();
            //    foreach (Type t in ts)
            //    {
            //        if (t.IsAbstract) continue;
            //        if (t.IsSubclassOf(typeof(CommandBase)))
            //        {
            //            var ins = Activator.CreateInstance(t) as CommandBase;
            //            Debug.Assert(t.BaseType != null, "t.BaseType != null");
            //            var entityType = t.BaseType.GetGenericArguments()[0];
            //            Debug.Assert(ins != null, "ins != null");
            //            _commands[ins.Name] = new RequestFactor()
            //            {
            //                Command = ins,
            //                PackageType = entityType,
            //                CreatePackageMethod = methodSerializer.MakeGenericMethod(entityType)
            //            };
            //            Logger.Info("load command:{0}", ins.Name);
            //        }
            //    }
            //}

            _pullSocket.Bind(Address + "in");
            _pushSocket.Connect(Address + "out");
            _mainThread.Start();
            _sendThread.Start();
            _receThread.Start();
            IsRunning = true;
            Logger.Info("game server start successful");
        }

        void OnSend()
        {
            IMessageEntity msg;
            if (_msgsWaiting.TryDequeue(out msg))
            {
                byte[] data = msg.Serialize();
                _pushSocket.Send(data);
            }
        }

        void OnReceive()
        {
            if (!_pullSocket.HasIn)
            {
                return;
            }
            byte[] data = _pullSocket.Receive();
            int offset = 0;
            var converter = EndianBitConverter.Big;
            long userID = converter.ToInt64(data, offset);
            offset += 8;
            int length = converter.ToUInt16(data, offset);
            offset += 2;
            int cmdName = converter.ToInt32(data, offset);
            offset += 4;
            RequestFactor factor;
            if (_commands.TryGetValue(cmdName.ToString("D"), out factor))
            {
                using (var stream = new MemoryStream())
                {
                    stream.Write(data, offset, length - 4);
                    stream.Position = 0;
                    var package = factor.CreatePackageMethod.Invoke(null, new object[] { stream });
                    _actionsWaiting.Enqueue(new Tuple<long, CommandBase, object>(userID,factor.Command, package));
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
            Tuple<long, CommandBase, object> tuple;
            while (_actionsWaiting.TryDequeue(out tuple))
            {
                Player p;
                if (tuple.Item2.Name == CMD_G2G.ToGameLogin.ToString("D"))
                {
                    p = new Player(tuple.Item1);
                    _allPlayers[tuple.Item1] = p;
                }
                else
                {
                    p = _allPlayers[tuple.Item1];
                }
                tuple.Item2.Execute(p, tuple.Item3);
            }
        }

        /// <summary>
        /// 发送给指定玩家报文
        /// </summary>
        public void Send<T>(Player player, int cmd, T package)
        {
            _msgsWaiting.Enqueue(new Message<T>(cmd, package, player.NetHandle));
        }

        /// <summary>
        /// 广播指定玩家
        /// </summary>
        public void Broadcast<T>(IEnumerable<Player> players, int cmd, T package)
        {
            _msgsWaiting.Enqueue(new Message<T>(cmd, package, players.Select(p=>p.NetHandle).ToArray()));
        }

        /// <summary>
        /// 广播全体玩家
        /// </summary>
        public void BroadcastAll<T>(int cmd, T package)
        {
            _msgsWaiting.Enqueue(new Message<T>(cmd, package, null));
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
            Logger.Info("game server stopped");
        }
    }

    public class RequestFactor
    {
        public CommandBase Command;
        public Type PackageType;
        public MethodInfo CreatePackageMethod;
    }
}
