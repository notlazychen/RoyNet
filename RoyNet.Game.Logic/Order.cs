using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoyNet.Game.Logic
{
    public enum ContainerName
    {
        Player,
        Module,
    }

    public class Order
    {
        /// <summary>
        /// 容器名
        /// </summary>
        public ContainerName ContainerName { get; set; }
        
        /// <summary>
        /// 索引
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; set; }
    }
}
