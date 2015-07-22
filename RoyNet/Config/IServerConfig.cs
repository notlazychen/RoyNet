using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;

namespace RoyNet
{
    public interface IServerConfig:INamedConfig
    {
        string ServerTypeName { get; }

        NameValueCollection Options { get; }
        NameValueCollection OptionElements { get; }
        TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new();
    }

    public interface INamedConfig
    {
        string Name { get; }
    }
}
