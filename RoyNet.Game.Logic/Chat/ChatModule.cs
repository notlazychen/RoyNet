using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoyNet.GameServer;
using RoyNet.GameServer.Logic.Chat;

namespace RoyNet.Game.Logic.Chat
{
    public class ChatModule:RoyNetModule
    {
        public override string Name
        {
            get { return "聊天模块"; }
        }

        public override void Startup(List<CommandBase> commandContainer)
        {
            commandContainer.Add(new ChatCommand());
        }
    }
}
