using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoyNet.GameServer.Entity;

namespace RoyNet.GameServer.Logic.Chat
{
    public class ChatCommand: CommandBase<Chat_Send>
    {
        public override string Name
        {
            get { return CMD_Chat.Send.ToString("D"); }
        }

        public override void OnExecute(Player player, Chat_Send msg)
        {
            Console.WriteLine(msg.Text);
            Server.Current.BroadcastAll((int)CMD_Chat.Send, new Chat_Send()
            {
                Text = string.Format("玩家{0}说：{1}", player.UserName, msg.Text)
            });
        }
    }
}
