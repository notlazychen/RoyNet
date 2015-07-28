using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace RoyNet
{
    public class Launcher
    {
        public static Launcher Create()
        {
            return new Launcher();
        }

        private readonly Dictionary<string, AppDomainServer> _servers = new Dictionary<string, AppDomainServer>();
        public IEnumerable<ServerBase> Servers { get { return _servers.Values.Select(s=>s.Server); } }

        public void Ignition()
        {
            var config = ConfigurationManager.GetSection("royNet") as RoyNetConfiguration;
            Debug.Assert(config != null, "royNet Config node is undefined");
            foreach (IServerConfig serverConfig in config.Servers)
            {
                var configSection = new ServerConfig(serverConfig);
                var type = config.ServerTypes[configSection.ServerTypeName].TypeProvider;

                AppDomain newDomain = AppDomain.CreateDomain(configSection.Name);
                var assembly = newDomain.Load(type.AssemblyName);
                var obj = newDomain.CreateInstanceAndUnwrap(type.AssemblyName, type.ClassFullName,
                        true,
                        BindingFlags.CreateInstance,
                        null,
                        null,
                        null,
                        new object[0]);
                var instance = obj as ServerBase;
                if (instance != null)
                {
                    instance.Configure(configSection);
                    _servers.Add(instance.Name, new AppDomainServer(newDomain, instance));
                }
            }
            //初始化完成
            //开始启动
            foreach (ServerBase serverBase in Servers)
            {
                serverBase.Startup();
            }
        }

        public void StopAll()
        {
            foreach (var appDomainServer in _servers.Values)
            {
                appDomainServer.Server.Stop();
                AppDomain.Unload(appDomainServer.AppDomain);
            }
            _servers.Clear();
        }

        public void Stop(string serverName)
        {
            AppDomainServer server = null;
            if (_servers.TryGetValue(serverName, out server))
            {
                server.Server.Stop();
                AppDomain.Unload(server.AppDomain);
                _servers.Remove(serverName);
            }
            //else
            //{
            //    throw new ArgumentException(string.Format("can't find the server named:{0}", serverName));
            //}
        }

        public void StartAll()
        {
            if (_servers.Count != 0)
            {
                throw new ArgumentException("Some servers are still running");
            }

            Ignition();
        }

        public void Start(string serverName)
        {
            if (_servers.ContainsKey(serverName))
            {
                throw new ArgumentException(string.Format("{0} has already been started", serverName));
            }

            var config = ConfigurationManager.GetSection("royNet") as RoyNetConfiguration;
            Debug.Assert(config != null, "royNet Config node is undefined");
            var serverConfig = config.Servers.OfType<IServerConfig>().FirstOrDefault(sc => sc.Name == serverName);
            if (serverConfig == null)
            {
                throw new ArgumentException(string.Format("can't find the server named:{0}", serverName));
            }

            var configSection = new ServerConfig(serverConfig);
            var type = config.ServerTypes[configSection.ServerTypeName].TypeProvider;

            AppDomain newDomain = AppDomain.CreateDomain(configSection.Name);
            var assembly = newDomain.Load(type.AssemblyName);
            var obj = newDomain.CreateInstanceAndUnwrap(type.AssemblyName, type.ClassFullName,
                    true,
                    BindingFlags.CreateInstance,
                    null,
                    null,
                    null,
                    new object[0]);
            var instance = obj as ServerBase;
            if (instance != null)
            {
                instance.Configure(configSection);
                _servers.Add(instance.Name, new AppDomainServer(newDomain, instance));
                instance.Startup();
            }
        }
    }
}
