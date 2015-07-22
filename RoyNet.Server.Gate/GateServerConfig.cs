using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyNet.Server.Gate
{
    class MessageQueueConfig : ConfigurationElement, INamedConfig
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
        }

        [ConfigurationProperty("consumers", IsRequired = true)]
        public ConfigurationElementCollection<Consumer> Customers
        {
            get { return (ConfigurationElementCollection<Consumer>)this["consumers"]; }
        }
    }

    class Consumer: ConfigurationElement, INamedConfig
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }
        
        [ConfigurationProperty("ip", IsRequired = true)]
        public string IP
        {
            get
            {
                return this["ip"] as string;
            }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
        }
    }

}
