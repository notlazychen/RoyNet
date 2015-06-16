using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoyNet.Game.Logic.Chat;
using RoyNet.GameServer;

namespace RoyNet.Game.Logic
{
    public class GameServerBootstrapper : RoyNetBootstrapper
    {
        public GameServerBootstrapper():base("ipc://game1")
        {
            
        }

        public GameServerBootstrapper(string address) : base(address)
        {

        }

        public override void ServerStartup(IList<RoyNetModule> modulesContainer)
        {
            base.ServerStartup(modulesContainer);
            modulesContainer.Add(new ChatModule());
        }
    }

    public class GameServer
    {
        private GameServerBootstrapper _bootstrapper;
        public static GameServer CreateBootstrapper(string address = null)
        {
            GameServer s = new GameServer();
            s._bootstrapper = new GameServerBootstrapper(address);
            return s;
        }

        public void Start()
        {
            _bootstrapper.GameServer.Open();
        }

        public void Stop()
        {
            _bootstrapper.GameServer.Close();
        }
    }
}
