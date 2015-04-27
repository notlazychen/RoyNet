using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Nancy;
using NetMQ;

namespace RoyNet.LoginServer
{
    public class LoginModule : NancyModule
    {
        public LoginModule()
        {
            Get["/login/{uname}/{pwd}"] = p =>
            {
                using (var conn = ConnectionProvider.Connection)
                {
                    string uid = conn.Query<string>("select uid from Account where username=@username and password=md5(@password)", new
                    {
                        username = p.uname,
                        password = p.pwd
                    }).FirstOrDefault();
                    if (uid != null)
                    {
                        string token = Guid.NewGuid().ToString();
                        SendToGate(uid, token);
                        return Response.AsJson(new { Result = "OK", Token = token });
                    }
                    else
                    {
                        return Response.AsJson(new { Result = "Failed" });
                    }
                }
            };

            Get["/reg/{uname}/{pwd}"] = p =>
            {
                using (var conn = ConnectionProvider.Connection)
                {
                    var account = new
                    {
                        username = p.uname,
                        password = p.pwd
                    };
                    int ret = conn.Execute("insert into Account(`username`,`password`) values(@username,@password)", account);
                    if (ret == 1)
                    {
                        string uid = conn.Query<string>("select uid from Account where username=@username", account).First();
                        string token = Guid.NewGuid().ToString();
                        SendToGate(uid, token);
                        return Response.AsJson(new { Result = "OK", Token = token });
                    }
                    else
                    {
                        return Response.AsJson(new { Result = "Failed" });
                    }
                }
            };
        }

        void SendToGate(string uid, string token)
        {
            using (var context = NetMQContext.Create())
            {
                using (var socket = context.CreateRequestSocket())
                {
                    socket.Connect(Config.GateAddress);
                    string msg = string.Format("{0},{1}", uid, token);
                    socket.Send(msg);
                    socket.Receive();//just wait
                }
            }
        }
    }
}
