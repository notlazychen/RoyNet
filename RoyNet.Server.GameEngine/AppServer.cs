using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiscUtil.Conversion;
using ProtoBuf;
using RoyNet.Server.GameEngine;
using RoyNet.Server.GameEngine.Logic.Login;
using RoyNet.Server.Gate.Entity;
using RoyNet.Util;

namespace RoyNet.Server.GameEngine
{
    public class AppServer : ServerBase, IDisposable
    {
        public static AppServer Current { get; private set; }
        private TcpClient _client2Gate;
        private NetworkStream _stream;
        public string GateServerIP { get; private set; }
        public int GateServerPort { get; private set; }

        private readonly TaskThread _mainThread;
        private readonly TaskThread _sendThread;
        private readonly TaskThread _receThread;
        private readonly ConcurrentQueue<Tuple<long,CommandBase, object>> _actionsWaiting = new ConcurrentQueue<Tuple<long, CommandBase, object>>();
        private readonly ConcurrentQueue<IMessageEntity> _msgsWaiting = new ConcurrentQueue<IMessageEntity>();
        private readonly Dictionary<string, RequestFactor> _commands = new Dictionary<string, RequestFactor>();

        public string PushAddress { get; private set; }
        public string PullAddress { get; private set; }

        public bool IsRunning { get; private set; }

        private readonly Dictionary<long, Player> _allPlayers = new Dictionary<long, Player>();
        public NLogLoggingService Logger { get; private set; }

        private readonly List<RoyNetModule> _modules = new List<RoyNetModule>();

        public IEnumerable<RoyNetModule> Modules
        {
            get { return _modules;}
        }

        public AppServer()
        {
            _mainThread = new TaskThread("执行", MainLoop);
            _sendThread = new TaskThread("发送", OnSend);
            _receThread = new TaskThread("接收", OnReceive);
            Current = this;
        }

        protected virtual void OnServerStartup(List<RoyNetModule> modulesContainer)
        {
            
        }

        void RegisterCommand(MethodInfo methodSerializer, CommandBase command)
        {
            Type t = command.GetType();
            Debug.Assert(t.BaseType != null, "t.BaseType != null");
            var entityType = t.BaseType.GetGenericArguments()[1];
            Debug.Assert(command != null, "ins != null");
            _commands[command.Name] = new RequestFactor()
            {
                Command = command,
                PackageType = entityType,
                CreatePackageMethod = methodSerializer.MakeGenericMethod(entityType)
            };
        }

        #region 服务器三巨头
        void OnSend()
        {
            try
            {
                IMessageEntity msg;
                if (_msgsWaiting.TryDequeue(out msg))
                {
                    byte[] data = msg.Serialize();
                    _stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        private byte[] _buffer = new byte[DefaultBufferSize];
        private const int DefaultBufferSize = 1024;

        void OnReceive()
        {
            try
            {
                if (_client2Gate == null || !_client2Gate.Connected)
                {
                    _client2Gate = new TcpClient();
                    _client2Gate.Connect(GateServerIP, GateServerPort);
                    _stream = _client2Gate.GetStream();
                }

                var buffer = _buffer;
                int size = _stream.Read(_buffer, 0, _buffer.Length);
                int totalSize = size;
                int offsetCpyed = 0;
                while (totalSize == _buffer.Length)
                {
                    var oldData = buffer;
                    buffer = new byte[buffer.Length * 2];
                    Buffer.BlockCopy(oldData, 0, buffer, 0, totalSize);
                    offsetCpyed += size;

                    size = _stream.Read(buffer, offsetCpyed, buffer.Length - offsetCpyed);
                    totalSize += size;
                    _buffer = buffer;
                }
                //拆包

                int offset = 0;
                var converter = EndianBitConverter.Big;

                while (offset < totalSize)
                {
                    long netHandle = converter.ToInt64(buffer, offset);
                    offset += 8;
                    int length = converter.ToUInt16(buffer, offset);
                    offset += 2;
                    int cmdName = converter.ToInt32(buffer, offset);
                    offset += 4;
                    RequestFactor factor;
                    if (_commands.TryGetValue(cmdName.ToString("D"), out factor))
                    {
                        var stream = new MemoryStream();
                        try
                        {
                            stream.Write(buffer, offset, length - 4);
                            offset += length - 4;
                            stream.Position = 0;
                            var package = factor.CreatePackageMethod.Invoke(null, new object[] {stream});
                            _actionsWaiting.Enqueue(new Tuple<long, CommandBase, object>(netHandle, factor.Command,
                                package));
                            stream.Dispose();
                        }
                        catch (ProtoException ex)
                        {
                            stream.Dispose();
                            //协议错误，考虑要不要断开客户端
                            Logger.Debug("unknow request:{0} from {1}", cmdName.ToString("D"), netHandle);
                        }
                    }
                    else
                    {
                        //不认识的协议，考虑要不要断开客户端
                        Logger.Debug("unknow request:{0} from {1}", cmdName.ToString("D"), netHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                _stream.Close();
                _client2Gate.Close();
                _client2Gate = null;
            }
        }

        /// <summary>
        /// 服务器上的每帧
        /// </summary>
        protected virtual void OnMainLoop(IEnumerable<Player> players)
        {
            
        }

        //游戏主循环
        void MainLoop()
        {
            try
            {
                Tuple<long, CommandBase, object> tuple;
                while (_actionsWaiting.TryDequeue(out tuple))
                {
                    Player p;
                    if (tuple.Item2.Name == CMD_G2G.ToGameConnect.ToString("D"))
                    {
                        p = new Player(tuple.Item1, this);
                        if (_allPlayers.ContainsKey(tuple.Item1))
                        {
                            _allPlayers[tuple.Item1].Kickout();//顶号
                        }
                        _allPlayers[tuple.Item1] = p;
                    } else 
                    if (tuple.Item2.Name == CMD_G2G.ToGameDisconnect.ToString("D"))
                    {
                        p = _allPlayers[tuple.Item1];
                        _allPlayers.Remove(tuple.Item1);
                    } else
                    {
                        p = _allPlayers[tuple.Item1];
                    }
                    tuple.Item2.Execute(this, p, tuple.Item3);
                }
                OnMainLoop(_allPlayers.Values);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        #endregion

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
        }

        /// <summary>
        /// 查找符合条件的在线玩家(顺序遍历)
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public Player FindOnLinePlayer(Func<Player,bool> selector)
        {
            return _allPlayers.Values.FirstOrDefault(selector);
        }

        protected override void OnConfigure(IServerConfig config)
        {
            Logger = new NLogLoggingService("roynet");
            Logger.Trace("************************");
            Logger.Trace("game server is starting");

            GateServerIP = config.Options["gateServerIP"];
            GateServerPort = int.Parse(config.Options["gateServerPort"]);

            List<CommandBase> commands = new List<CommandBase>()
            {
                new LoginCommand(),
                new LogoutCommand()
            };

            //加载模块
            OnServerStartup(_modules);
            foreach (RoyNetModule module in _modules)
            {
                module.Configure(commands);
                Logger.Trace("load module:{0}", module.Name);
            }

            //初始化Command
            MethodInfo methodSerializer = typeof(Serializer).GetMethod("Deserialize");
            foreach (CommandBase command in commands)
            {
                RegisterCommand(methodSerializer, command);
                Logger.Trace("load command:{0}", command.Name);
            }

        }

        protected override void OnStart()
        {
            _mainThread.Start();
            _sendThread.Start();
            _receThread.Start();
            IsRunning = true;
            Logger.Trace("game server start successful");

            foreach (RoyNetModule module in _modules)
            {
                module.Startup();
            }
        }

        protected override void OnStop()
        {
            if (!IsRunning)
                return;

            _mainThread.Stop();
            _sendThread.Stop();
            _receThread.Stop();

            _stream.Dispose();
            _client2Gate.Close();

            IsRunning = false;
            Logger.Trace("game server stopped");
        }

        protected override void OnRequire()
        {
            throw new NotImplementedException();
        }
    }

    public class RequestFactor
    {
        public CommandBase Command;
        public Type PackageType;
        public MethodInfo CreatePackageMethod;
    }
}
