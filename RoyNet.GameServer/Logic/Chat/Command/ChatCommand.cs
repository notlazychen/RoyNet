using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoyNet.GameServer.Entity;

namespace RoyNet.GameServer.Logic.Chat
{
    public class ChatCommand: CommandBase<C2S_Chat_Send>
    {
        public override string Name
        {
            get { return CMD_Chat.C2S_Send.ToString("D"); }
        }

        public override void OnExecute(Player player, C2S_Chat_Send msg)
        {
            Console.WriteLine(msg.Text);
        }
    }
}
