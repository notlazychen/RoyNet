using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiscUtil.Conversion;
using NetMQ;
using NetMQ.zmq;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace RoyNet.GateServer
{
    /// <summary>
    /// 游戏中的互动操作
    /// </summary>
    public class InteractCommand : ICommand<PlayerSession, BinaryRequestInfo>
    {
        public string Name
        {
            get { return 0x02.ToString("X"); }
        }

        public void ExecuteCommand(PlayerSession session, BinaryRequestInfo requestInfo)
        {
            //if (!session.IsLogin)
            //    return;//未登录的过滤
            //todo: 转发给游戏服
            var converter = EndianBitConverter.Big;
            byte[] data = new byte[requestInfo.Body.Length + 6];
            int offset = 0;
            converter.CopyBytes(session.UserID, data, 0);
            offset += 4;
            converter.CopyBytes((ushort)requestInfo.Body.Length, data, offset);
            offset += 2;
            Buffer.BlockCopy(requestInfo.Body, 0, data, offset, requestInfo.Body.Length);
            session.Server.Push(data);
            Console.WriteLine("收到报文{0}，转发游戏服", Name);
        }
    }
}
