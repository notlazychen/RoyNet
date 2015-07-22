using RoyNet.Server.GameEngine;
using RoyNet.Server.GameEngine.Entity;

namespace RoyNet.Server.Game.Chat
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
            GameEngine.AppServer.Current.BroadcastAll((int)CMD_Chat.Send, new Chat_Send()
            {
                Text = string.Format("玩家{0}说：{1}", player.UserName, msg.Text)
            });
        }
    }
}
