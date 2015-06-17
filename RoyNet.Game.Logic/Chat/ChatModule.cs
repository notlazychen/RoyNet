using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoyNet.GameServer;

namespace RoyNet.Game.Logic.Chat
{
    public class ChatModule:RoyNetModule
    {
        public override string Name
        {
            get { return "聊天模块"; }
        }

        public override void Startup()
        {
        }

        public override void Configure(List<CommandBase> commandContainer)
        {
            commandContainer.Add(new ChatCommand());
        }
    }
}
