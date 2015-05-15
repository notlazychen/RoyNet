using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiscUtil.Conversion;
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
            string token = Encoding.UTF8.GetString(requestInfo.Body, 4, requestInfo.Body.Length - 4);
            string url = string.Format("{0}chk/{1}", session.Server.LoginServerAddress, token);
            var request = WebRequest.Create(url);
            try
            {
                var response = request.GetResponse();
                var stream = response.GetResponseStream();
                byte[] data = new byte[64];
                if (stream != null && stream.CanRead)
                {
                    int length = stream.Read(data, 0, 64);
                    string result = Encoding.UTF8.GetString(data, 0, length);
                    var lr = JsonConvert.DeserializeObject<LoginResult>(result);
                    if (lr.Result == "OK")//验证成功
                    {
                        if (session.IsLogin)
                            return;//重复登录

                        //检查该UID玩家是否在线
                        var sameSession = session.Server.GetAllSessions().FirstOrDefault(s => s.UID == lr.UID);
                        if (sameSession != null)
                        {
                            //挤掉
                            sameSession.Close();
                        }

                        session.Login(lr.UID);
                        var package = new G2G_ToGameLogin()
                        {
                            UserName = lr.UID
                        };
                        using (var ms = new MemoryStream())
                        {
                            Serializer.Serialize(ms, package);
                            var loginPackage = ms.ToArray();

                            var converter = EndianBitConverter.Big;
                            byte[] sendData = new byte[loginPackage.Length + 4];
                            converter.CopyBytes((int)CMD_G2G.ToGameLogin, sendData, 0);
                            Buffer.BlockCopy(loginPackage, 0, sendData, 4, loginPackage.Length);
                            session.Server.Push2GameServer(session, sendData);
                        }
                        session.Send(new ArraySegment<byte>(new byte[] { 1 }));
                    }
                    else
                    {
                        session.Close(CloseReason.ServerClosing);
                    }
                }
            }
            catch (Exception ex)
            {
                session.Logger.Debug(string.Format("SessionID:{0},Exception:{1}", session.SessionID, ex.Message));
            }
        }

        public class LoginResult
        {
            public string UID { get; set; }
            public string Result { get; set; }
        }
    }
}
