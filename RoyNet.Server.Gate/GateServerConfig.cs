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

        [ConfigurationProperty("dest", IsRequired = true)]
        public int Dest
        {
            get { return (int)this["dest"]; }
        }

        [ConfigurationProperty("allowedIPArray", IsRequired = false)]
        public string AllowedIPArray
        {
            get { return (string)this["allowedIPArray"]; }
        }

        //[ConfigurationProperty("maxBufferSize", IsRequired = false)]
        //public int MaxBufferSize
        //{
        //    get { return (int) this["maxBufferSize"]; }
        //}
    }

}
