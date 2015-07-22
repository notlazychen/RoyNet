using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace RoyNet.Server.Gate
{
    class GatewayReceiveFilter : FixedHeaderReceiveFilter<BinaryRequestInfo>
    {
        public GatewayReceiveFilter()
            : base(6)
        {
            
        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            //byte salt1 = header[offset];    //备用
            //byte salt2 = header[offset + 1];//备用
            //byte dest = header[offset + 2]; //目标
            //byte cmd  = header[offset + 3]; //Command Name
            int len = header[offset + 4] * 256 + header[offset + 5];//最后两字节表示长度
            return len;
        }

        protected override BinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            byte[] bodydata = new byte[length];
            Buffer.BlockCopy(bodyBuffer, offset, bodydata, 0, length);
            return new BinaryRequestInfo(header.Array[header.Offset + 3].ToString("X"), bodydata);
        }
    }

    public class GatewayReceiveFilterFactory : IReceiveFilterFactory<BinaryRequestInfo>
    {
        public IReceiveFilter<BinaryRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            return new GatewayReceiveFilter();
        }
    }
}
