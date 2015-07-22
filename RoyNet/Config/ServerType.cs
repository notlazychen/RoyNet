using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace RoyNet
{
    public class ServerType : ConfigurationElement, INamedConfig
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return this["type"] as string;
            }
        }

        public TypeProvider TypeProvider
        {
            get { return TypeProvider.Parse(Type);}
        }
    }

    public class TypeProvider
    {
        public TypeProvider(string classFullName, string ns)
        {
            ClassFullName = classFullName;
            AssemblyName = ns;
        }

        public string ClassFullName { get; private set; }
        public string AssemblyName { get; private set; }

        public static TypeProvider Parse(string str)
        {
            string[] tmp = str.Split(',');
            if (tmp.Length != 2)
            {
                throw new ArgumentException("错误的类型配置格式，请确保用半角逗号分割，前段为类型完全限定名，后段为命名空间");
            }
            var inst = new TypeProvider(tmp[0].Trim(' '), tmp[1].Trim(' '));
            return inst;
        }
    }
}
