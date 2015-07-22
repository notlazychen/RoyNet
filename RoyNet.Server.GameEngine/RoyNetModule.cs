using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoyNet.Server.GameEngine
{
    public abstract class RoyNetModule
    {
        public abstract string Name { get; }

        /// <summary>
        /// 开始驱动
        /// </summary>
        public abstract void Startup();

        /// <summary>
        /// 配置Command
        /// </summary>
        public abstract void Configure(List<CommandBase> commandContainer);
    }
}
