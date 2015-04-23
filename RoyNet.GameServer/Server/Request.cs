using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using RoyNet.Engine;

namespace RoyNet.GameServer
{
    ///// <summary>
    ///// 接收请求
    ///// </summary>
    //public class Request<T> :IRequestEntity<T>
    //    where T: class
    //{
    //    public int CommandID { get; set; }
    //    public T Entity { get; set; }

    //    public void Deserialize(byte[] data)
    //    {
    //        using (MemoryStream stream = new MemoryStream())
    //        {
    //            stream.Write(data, 0, data.Length);
    //            stream.Position = 0;
    //            Entity = Serializer.Deserialize<T>(stream);
    //        }
    //    }
    //}
}
