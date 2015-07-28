using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoyNet
{
    class AppDomainServer
    {
        public AppDomain AppDomain { get; private set; }

        public ServerBase Server { get; private set; }

        public AppDomainServer(AppDomain appDomain, ServerBase server)
        {
            AppDomain = appDomain;
            Server = server;
        }

        //public void Stop()
        //{
        //    Server.Stop();
        //    //AppDomain.Unload(AppDomain);
        //}

        //public void Start()
        //{
            
        //}
    }
}
