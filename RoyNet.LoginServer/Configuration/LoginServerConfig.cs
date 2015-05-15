using System.Collections.Generic;
using System.Configuration;

namespace RoyNet.LoginServer
{
    class LoginServerConfig : ConfigurationSection
    {
        [ConfigurationProperty("port")]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        private List<GameServer> _gameServers;
        public IEnumerable<GameServer> GameServers
        {
            get
            {
                if (_gameServers == null)
                {
                    _gameServers = new List<GameServer>();
                    foreach (GameServerElement server in GameServerElements)
                    {
                        _gameServers.Add(new GameServer()
                        {
                            DestID = server.DestID,
                            IP = server.IP,
                            Name = server.Name,
                            Port = server.Port
                        });
                    }
                }
                return _gameServers;
            }
        }

        [ConfigurationProperty("gameServers")]
        public GameServerElements GameServerElements
        {
            get
            {
                return this["gameServers"] as GameServerElements;
            }
            set
            {
                this["gameServers"] = value;
            }
        }
    }

    class GameServer
    {
        public string Name { get; set; }

        public string IP { get; set; }

        public int Port { get; set; }

        public int DestID { get; set; }
    }

    public class GameServerElements : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new GameServerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GameServerElement)element).Name;
        }

        public GameServerElement this[string name]
        {
            get
            {
                return BaseGet(name) as GameServerElement;
            }
        }
    }

    public class GameServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("destID")]
        public int DestID
        {
            get
            {
                return (int)this["destID"];
            }
            set
            {
                this["destID"] = value;
            }
        }

        [ConfigurationProperty("ip")]
        public string IP
        {
            get
            {
                return this["ip"] as string;
            }
            set
            {
                this["ip"] = value;
            }
        }

        [ConfigurationProperty("port")]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }
    }
}
