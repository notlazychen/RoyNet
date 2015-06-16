using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoyNet.GameServer
{
    public abstract class RoyNetModule
    {
        public abstract string Name { get; }

        public abstract void Startup(List<CommandBase> commandContainer);
    }
}
