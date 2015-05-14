using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyNet.GameServer
{
    public class Player
    {
        public Server GameServer { get; private set; }
        public readonly long NetHandle;
        public string UserName { get; private set; }

        public Player(long netHandle)
        {
            NetHandle = netHandle;
        }

        public void Login(string username)
        {
            UserName = username;
        }

        public void Send<T>(int cmd, T package)
        {
            GameServer.Send(this,cmd, package);
        }
    }
}
