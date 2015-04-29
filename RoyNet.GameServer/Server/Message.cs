using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiscUtil.Conversion;
using ProtoBuf;
using RoyNet.Engine;

namespace RoyNet.GameServer
{
    /// <summary>
    /// 发送报文
    /// </summary>
    public class Message<T> : IMessageEntity<T>
    {
        private readonly long[] _netHandles;

        public int CommandID { get; private set; }
        public T Entity { get; set; }

        public Message()
        {
        }

        /// <summary>
        /// 发送给指定玩家报文
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="entity"></param>
        /// <param name="netHandles">null表示全部玩家</param>
        public Message(int cmd, T entity, params long[] netHandles)
        {
            _netHandles = netHandles;
            CommandID = cmd;
            Entity = entity;
        }

        public byte[] Serialize()
        {
            return InternalSerialize();
        }

        private byte[] InternalSerialize()
        {
            byte[] buffEntity;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, Entity);
                buffEntity = stream.ToArray();
            }
            var entityLen = buffEntity.Length;

            var converter = EndianBitConverter.Big;
            int userscount = 0;
            if (_netHandles != null)
                userscount = _netHandles.Length;
            var data = new byte[4 + userscount*4 + 2 + 4 + buffEntity.Length];
            var offset = 0;
            //首先是头
            converter.CopyBytes(userscount, data, 0);
            offset += 4;
            if (_netHandles != null)
            {
                foreach (var netHandle in _netHandles)
                {
                    converter.CopyBytes(netHandle, data, offset);
                    offset += 8;
                }
            }

            //然后是Body
            converter.CopyBytes((ushort)(entityLen + 4), data, offset);
            offset += 2;
            converter.CopyBytes(CommandID, data, offset);
            offset += 4;
            Buffer.BlockCopy(buffEntity, 0, data, offset, entityLen);
            return data;
        }
        /*
         * 返回客户端报文格式：
         * 4字节指向客户端数量（在网关处卸掉）
         * 根据客户端数量取走头(在网关处卸掉)x8long
         * 4字节CommandID
         * 2字节Body长度
         * Body（proto-buf）
         */
    }
}
