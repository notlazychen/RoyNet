using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoyNet.GameServer
{
    public abstract class RoyNetBootstrapper
    {
        public Server GameServer { get; private set; }
        
        protected RoyNetBootstrapper(string address)
        {
            GameServer = new Server(address);
        }

        public virtual void ServerStartup(IList<RoyNetModule> modulesContainer)
        {
            
        }
    }
}
