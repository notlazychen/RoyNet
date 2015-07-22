using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoyNet.Server.Gate.Entity;

namespace RoyNet.Server.GameEngine.Logic.Login
{
    internal class LogoutCommand : CommandBase<AppServer,G2G_ToGameDisconnect>
    {
        public override string Name
        {
            get { return CMD_G2G.ToGameDisconnect.ToString("D"); }
        }
        
        public override void OnExecute(AppServer server, Player player, G2G_ToGameDisconnect msg)
        {
            player.SetLogout();
        }
    }
}
