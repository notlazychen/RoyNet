using System.Collections.Generic;
using RoyNet.Game.Logic.Chat;
using RoyNet.GameServer;

namespace RoyNet.Game.Logic
{
    public class GameServer : Server
    {
        public GameServer(string address)
            : base(address)
        {
        }

        protected override void OnServerStartup(List<RoyNetModule> modulesContainer)
        {
            base.OnServerStartup(modulesContainer);
            modulesContainer.Add(new ChatModule());
        }
    }
}
