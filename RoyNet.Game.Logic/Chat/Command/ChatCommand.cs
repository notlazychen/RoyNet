using RoyNet.GameServer;
using RoyNet.GameServer.Entity;

namespace RoyNet.Game.Logic.Chat
{
    public class ChatCommand: CommandBase<GameServer,Chat_Send>
    {
        public override string Name
        {
            get { return CMD_Chat.Send.ToString("D"); }
        }

        public override void OnExecute(GameServer server, Player player, Chat_Send msg)
        {
            //Console.WriteLine(msg.Text);
            Server.Current.BroadcastAll((int)CMD_Chat.Send, new Chat_Send()
            {
                Text = string.Format("玩家{0}说：{1}", player.UserName, msg.Text)
            });
        }
    }
}
