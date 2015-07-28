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
            //一个群发示例
            server.BroadcastAll((int)CMD_Chat.Send, new Chat_Send()
            { 
                Text = msg.Text//string.Format("玩家{0}说：{1}", player.UserName, msg.Text)
            });

            //一个指定发送的示例
            Player player2 = server.FindOnLinePlayer(p => p.UserName == "0");
            if (player2 != null)
            {
                player2.Send((int)CMD_Chat.Send, new Chat_Send()
                {
                    Text = string.Format("玩家{0}对你说：{1}", player.UserName, msg.Text)
                });
            }
            else
            {
                server.Logger.Trace("玩家【0】并不存在");
            }
        }
    }
}
