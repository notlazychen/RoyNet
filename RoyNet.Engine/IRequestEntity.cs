using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyNet.Engine
{
    public interface IRequestEntity
    {
        void Deserialize(byte[] data);
    }

    public interface IRequestEntity<T> : IRequestEntity
    {
        T Entity { get; set; }
    }
}
