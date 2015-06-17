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
            if (session.IsLogin)
            {
#if Log
                Console.WriteLine("重复登录");
#endif
                return;//重复登录
            }

            string token = Encoding.UTF8.GetString(requestInfo.Body, 4, requestInfo.Body.Length - 4);
            
#if Log
            Console.WriteLine("有玩家尝试登录，token: {0}", token);
#endif

            string url = string.Format("{0}chk/{1}", session.Server.LoginServerAddress, token);
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
#if Log
                                    Console.WriteLine("异地登录，踢下线");
#endif
                                    sameSession.Close(CloseReason.ServerClosing);
                                }

                                session.EnterGame(lr);
                                session.Send(new ArraySegment<byte>(new byte[] {1}));
                            }
                            else
                            {
#if Log
                                Console.WriteLine("验证失败，踢下线");
#endif
                                session.Close(CloseReason.ServerClosing);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
#if Log
                Console.WriteLine("SessionID:{0},Exception:{1}", session.SessionID, ex.Message);
#endif
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
