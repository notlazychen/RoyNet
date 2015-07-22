using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace RoyNet
{
    [Serializable]
    public class ServerConfig : IServerConfig
    {
        public ServerConfig(IServerConfig config)
        {
            Name = config.Name;
            ServerTypeName = config.ServerTypeName;
            Options = config.Options;
            OptionElements = config.OptionElements;
        }

        public string Name { get; private set; }
        public string ServerTypeName { get; private set; }
        public NameValueCollection Options { get; private set; }
        public NameValueCollection OptionElements { get; private set; }

        public virtual TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new()
        {
            return this.OptionElements.GetChildConfig<TConfig>(childConfigName);
        }
    }

    [Serializable]
    public class ConfigurationElementBase : ConfigurationElement, IServerConfig
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("serverTypeName", IsRequired = false)]
        public string ServerTypeName
        {
            get
            {
                return this["serverTypeName"] as string;
            }
        }

        private readonly NameValueCollection _options = new NameValueCollection();
        private readonly NameValueCollection _optionElments = new NameValueCollection();
        public NameValueCollection Options { get { return _options; } }
        public NameValueCollection OptionElements { get { return _optionElments; } }
        public TConfig GetChildConfig<TConfig>(string childConfigName) where TConfig : ConfigurationElement, new()
        {
            return this.OptionElements.GetChildConfig<TConfig>(childConfigName);
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            Options.Add(name, value);
            return true;
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            OptionElements.Add(elementName, reader.ReadOuterXml());
            return true;
        }
    }
}
