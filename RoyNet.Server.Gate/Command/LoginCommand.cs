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
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace RoyNet.Server.Gate
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
            if (session.IsLogin)
            {
                session.Logger.Debug("重复登录");
                return;//重复登录
            }

            string token = Encoding.UTF8.GetString(requestInfo.Body, 4, requestInfo.Body.Length - 4);
            
            session.Logger.DebugFormat("有玩家尝试登录，token: {0}", token);

            string url = string.Format("{0}/{1}", session.Server.LoginServerUrl, token);
            var request = WebRequest.Create(url);
            try
            {
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        byte[] data = new byte[64];
                        if (stream != null && stream.CanRead)
                        {
                            int length = stream.Read(data, 0, 64);
                            string result = Encoding.UTF8.GetString(data, 0, length);
                            LoginResult lr = JsonConvert.DeserializeObject<LoginResult>(result);
                            if (lr.Result == "OK") //验证成功
                            {

                                //检查该UID玩家是否在线
                                var sameSession = session.Server.GetAllSessions().FirstOrDefault(s => s.UID == lr.UID);
                                if (sameSession != null)
                                {
                                    //挤掉
                                    session.Logger.DebugFormat("[{0}]{1}", lr.UID, "异地登录，踢下线");
                                    sameSession.Close(CloseReason.ServerClosing);
                                }

                                session.EnterGame(lr);
                                session.Send(new ArraySegment<byte>(new byte[] {1}));
                            }
                            else
                            {
                                session.Logger.Debug("验证失败，踢下线");
                                session.Close(CloseReason.ServerClosing);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                session.Logger.Error(string.Format("SessionID:{0},Exception:{1}", session.SessionID, ex.Message), ex);
            }
        }

        public class LoginResult
        {
            public string UID { get; set; }
            public string Result { get; set; }
        }
    }
}
