using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using WebMatrix.Data;

namespace RoyNet.LoginServer
{
    public class LoginModule:NancyModule
    {
        public LoginModule()
        {
            Get["/{uname}/{pwd}"] = p =>
            {
                using (Database db = Database.Open("db"))
                {
                    var uid = db.QueryValue("select uid from t_users where username=@0 and password=PASSWORD(@1)", p.uname, p.pwd);//看到这行数据库表结构是怎样的我就不多说了
                    if (uid != null)
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
