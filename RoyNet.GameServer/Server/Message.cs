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
        private readonly int[] _users;

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
        /// <param name="users">-1表示全部玩家</param>
        public Message(int cmd, T entity, params int[] users)
        {
            _users = users;
            CommandID = cmd;
            Entity = entity;
        }

        public ArraySegment<byte> Serialize()
        {
            return InternalSerialize();
        }

        private ArraySegment<byte> InternalSerialize()
        {
            byte[] buffEntity;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, Entity);
                buffEntity = stream.ToArray();
            }
            var entityLen = buffEntity.Length;

            var converter = EndianBitConverter.Big;
            var data = new byte[4 + _users.Length * 4 + 2 + 4 + buffEntity.Length];
            var offset = 0;
            //首先是头
            converter.CopyBytes(_users.Count(), data, 0);
            offset += 4;
            foreach (var user in _users)
            {
                converter.CopyBytes(user, data, offset);
                offset += 4;
            }

            //然后是Body
            converter.CopyBytes((ushort)(entityLen + 4), data, offset);
            offset += 2;
            converter.CopyBytes(CommandID, data, offset);
            offset += 4;
            Buffer.BlockCopy(buffEntity, 0, data, offset, entityLen);
            return new ArraySegment<byte>(data, 0, offset + entityLen);
        }
        /*
         * 返回客户端报文格式：
         * 4字节指向客户端数量（在网关处卸掉）
         * 根据客户端数量取走头(在网关处卸掉)
         * 4字节CommandID
         * 2字节Body长度
         * Body（proto-buf）
         */
    }
}
