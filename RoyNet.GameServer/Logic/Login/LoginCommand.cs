using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoyNet.GateServer.Entity;

namespace RoyNet.GameServer.Logic.Login
{
    internal class LoginCommand : CommandBase<Server, G2G_ToGameConnect>
    {
        public override string Name
        {
            get { return CMD_G2G.ToGameConnect.ToString("D"); }
        }
        
        public override void OnExecute(Server server, Player player, G2G_ToGameConnect msg)
        {
            player.SetLogin(msg.UserName);
        }
    }
}
