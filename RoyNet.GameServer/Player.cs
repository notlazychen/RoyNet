using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoyNet.GateServer.Entity;

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

        public void SetLogin(string username)
        {
            UserName = username;
        }

        public void SetLogout()
        {
            
        }

        public void Kickout()
        {
            GameServer.Send(this, (int)CMD_G2G.ToGateLeave, new G2G_ToGateLeave()
            {
                Reason = LeaveReason.Displace 
            });
        }

        public void Send<T>(int cmd, T package)
        {
            GameServer.Send(this,cmd, package);
        }
    }
}
