using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoyNet.Server.GameEngine;

namespace RoyNet.Server.Game.Chat
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
