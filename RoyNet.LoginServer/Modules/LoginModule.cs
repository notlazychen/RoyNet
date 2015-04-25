using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Nancy;

namespace RoyNet.LoginServer
{
    public class LoginModule:NancyModule
    {
        public LoginModule()
        {
            Get["/{uname}/{pwd}"] = p =>
            {
                using (var conn = ConnectionProvider.Connection)
                {
                    string id = conn.Query<string>("select uid from t_user where username=@username and password=md5(@password)", new
                    {
                        username = p.uname,
                        password = p.pwd
                    }).FirstOrDefault();//看到这行数据库表结构是怎样的我就不多说了
                    if (id != null)
                    {
                        Guid token = Guid.NewGuid();
                        //todo: 发送给网关服务器
                        //...
                        return Response.AsJson(new { Result = "OK", Token = token });
                    }
                    else
                    {
                        return Response.AsJson(new { Result = "Failed" });
                    }
                }
            };
        }
    }
}
