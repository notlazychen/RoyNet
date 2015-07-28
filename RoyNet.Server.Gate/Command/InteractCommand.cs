using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace RoyNet.Server.Gate
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
            if (!session.IsLogin)
                return;//未登录的过滤
            //转发给游戏服
            session.Server.Push2GameServer(session, requestInfo.Body);
            //Console.WriteLine("收到报文{0}，转发游戏服", Name);
        }
    }
}
