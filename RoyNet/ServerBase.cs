using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace RoyNet
{
    public abstract class ServerBase : MarshalByRefObject
    {
        public void Configure(IServerConfig config)
        {
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
