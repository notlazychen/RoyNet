using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RoyNet.Server.GameEngine;
using RoyNet.Server.Game.Chat;

namespace RoyNet.Server.Game
{
    public class GameServer : GameEngine.AppServer
    {
        public GameServer() { }
        protected override void OnServerStartup(List<RoyNetModule> modulesContainer)
        {
            base.OnServerStartup(modulesContainer);
            modulesContainer.Add(new ChatModule());
        }

        protected override void OnMainLoop(IEnumerable<Player> players)
        {
            base.OnMainLoop(players);
        }
    }
}
