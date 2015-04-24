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
        /// <param name="users">null表示全部玩家</param>
        public Message(int cmd, T entity, params int[] users)
        {
            _users = users;
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
            if (_users != null)
                userscount = _users.Length;
            var data = new byte[4 + userscount*4 + 2 + 4 + buffEntity.Length];
            var offset = 0;
            //首先是头
            converter.CopyBytes(userscount, data, 0);
            offset += 4;
            if (_users != null)
            {
                foreach (var user in _users)
                {
                    converter.CopyBytes(user, data, offset);
                    offset += 4;
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
         * 根据客户端数量取走头(在网关处卸掉)
         * 4字节CommandID
         * 2字节Body长度
         * Body（proto-buf）
         */
    }
}
