using System.Configuration;

namespace RoyNet
{
    public class ConfigurationElementCollection<T> : ConfigurationElementCollection
        where T : ConfigurationElement, INamedConfig, new()
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((INamedConfig)element).Name;
        }

        public new T this[string name]
        {
            get
            {
                return BaseGet(name) as T;
            }
        }
    }
}
