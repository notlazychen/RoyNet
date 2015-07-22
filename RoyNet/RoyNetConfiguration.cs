using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;

namespace RoyNet
{
    public class RoyNetConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("servers")]
        public ServerConfigCollection Servers
        {
            get
            {
                return this["servers"] as ServerConfigCollection;
            }
        }

        [ConfigurationProperty("serverTypes")]
        public ServerTypeCollection ServerTypes
        {
            get
            {
                return this["serverTypes"] as ServerTypeCollection;
            }
        }
    }
}
