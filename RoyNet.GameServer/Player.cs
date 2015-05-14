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
        public long UserID { get; private set; }

        public void Send<T>(int cmd, T package)
        {
            GameServer.Send(this,cmd, package);
        }
    }
}
