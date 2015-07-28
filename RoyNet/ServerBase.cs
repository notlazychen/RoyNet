using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace RoyNet
{
    public abstract class ServerBase : MarshalByRefObject
    {
        public string Name { get; private set; }
        public IServerConfig Config;

        public void Configure(IServerConfig config)
        {
            Name = config.Name;
            Config = config;
            OnConfigure(config);
        }

        public void Startup()
        {
            OnStart();
        }

        public void Require(string order)
        {
            OnRequire();
        }

        public void Stop()
        {
            OnStop();
        }


        protected abstract void OnConfigure(IServerConfig config);
        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract void OnRequire();
    }
}
