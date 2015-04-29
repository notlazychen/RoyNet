using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf;
using RoyNet.GateServer.Entity;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace RoyNet.GateServer
{
    /// <summary>
    /// 登录操作
    /// </summary>
    public class LoginCommand : ICommand<PlayerSession, BinaryRequestInfo>
    {
        public string Name
        {
            get { return 0x01.ToString("X"); }
        }

        public void ExecuteCommand(PlayerSession session, BinaryRequestInfo requestInfo)
        {
            string token = Encoding.UTF8.GetString(requestInfo.Body);
            string url = string.Format("{0}check/{1}", session.Server.LoginServerAddress, token);
            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            byte[] data = new byte[64];
            if (stream != null && stream.CanRead)
            {
                int length = stream.Read(data, 0, 64);
                string result = Encoding.UTF8.GetString(data, 0, length);
                var lr = JsonConvert.DeserializeObject<LoginResult>(result);
                if (lr.ResultCode == 0)
                {
                    session.Login();
                    var package = new G2G_ToGameLogin()
                    {
                        UserName = lr.UserName
                    };
                    using (var ms = new MemoryStream())
                    {
                        Serializer.Serialize(ms, package);
                        var sendData = ms.ToArray();
                        session.Server.Push2GameServer(session, sendData);
                    }
                }
                else
                {
                    session.Close(CloseReason.ServerClosing);
                }
            }
        }

        public class LoginResult
        {
            public string UserName { get; set; }
            public int ResultCode { get; set; }
        }
    }
}
