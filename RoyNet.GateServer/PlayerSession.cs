using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiscUtil.Conversion;
using ProtoBuf;
using RoyNet.GateServer.Entity;
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
        public string UID { get; private set; }

        public GatewayServer Server { get; private set; }

        protected override void OnSessionStarted()
        {
            Server = AppServer as GatewayServer;
            base.OnSessionStarted();
#if Log
            Console.WriteLine("{0}:one session started", DateTime.Now);
#endif
        }
        
        public void EnterGame(LoginCommand.LoginResult lr)
        {
            _isLogin = true;
            UID = lr.UID;
            _netHandle = Interlocked.Increment(ref _netHandleAutoIncrease);

            var package = new G2G_ToGameConnect()
            {
                UserName = lr.UID,
                IPAddress = RemoteEndPoint.Address.ToString(),
                Port = RemoteEndPoint.Port,
            };
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, package);
                var loginPackage = ms.ToArray();

                var converter = EndianBitConverter.Big;
                byte[] sendData = new byte[loginPackage.Length + 4];
                converter.CopyBytes((int)CMD_G2G.ToGameConnect, sendData, 0);
                Buffer.BlockCopy(loginPackage, 0, sendData, 4, loginPackage.Length);
                Server.Push2GameServer(this, sendData);
            }
        }
    }
}
