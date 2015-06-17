using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoyNet.GateServer.Entity;

namespace RoyNet.GameServer.Logic.Login
{
    internal class LogoutCommand : CommandBase<Server,G2G_ToGameDisconnect>
    {
        public override string Name
        {
            get { return CMD_G2G.ToGameDisconnect.ToString("D"); }
        }
        
        public override void OnExecute(Server server, Player player, G2G_ToGameDisconnect msg)
        {
            player.SetLogout();
        }
    }
}
