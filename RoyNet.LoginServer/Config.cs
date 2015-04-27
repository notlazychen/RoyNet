using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyNet.LoginServer
{
    static class Config
    {
        public static readonly string HostUrl = ConfigurationManager.AppSettings["HostUrl"];
        public static readonly string GateAddress = ConfigurationManager.AppSettings["GateAddress"];
    }
}
