using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoyNet.Server.Gate.Entity;

namespace RoyNet.Server.GameEngine.Logic.Login
{
    internal class LoginCommand : CommandBase<AppServer, G2G_ToGameConnect>
    {
        public override string Name
        {
            get { return CMD_G2G.ToGameConnect.ToString("D"); }
        }
        
        public override void OnExecute(AppServer server, Player player, G2G_ToGameConnect msg)
        {
            player.SetLogin(msg.UserName);
        }
    }
}
