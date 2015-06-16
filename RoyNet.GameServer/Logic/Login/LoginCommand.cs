using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoyNet.GateServer.Entity;

namespace RoyNet.GameServer.Logic.Login
{
    internal class LoginCommand : CommandBase<G2G_ToGameLogin>
    {
        public override string Name
        {
            get { return CMD_G2G.ToGameLogin.ToString("D"); }
        }

        public override void OnExecute(Player player, G2G_ToGameLogin msg)
        {
            player.Login(msg.UserName);
        }
    }
}
