using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            //todo: 下面是假象，完成登录验证
            var bs = Guid.NewGuid().ToByteArray();
            bool same = true;
            if (requestInfo.Body.Length == bs.Length)
            {
                if (bs.Where((t, i) => t != requestInfo.Body[i]).Any())
                {
                    same = false;
                }
            }
            else
            {
                same = false;
            }
            if (same)
            {
                session.IsLogin = true;
            }
            //Console.WriteLine("收到报文{0}，执行结果{1}", Name, same);
        }
    }
}
