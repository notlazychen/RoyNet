using System;

namespace RoyNet.Engine
{
    public interface IMessageEntity
    {
        int CommandID { get; }
        byte[] Serialize();
    }

    public interface IMessageEntity<T> : IMessageEntity
    {
        T Entity { get; set; }
    }
}
