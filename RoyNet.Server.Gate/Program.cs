using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;

namespace RoyNet.Server.Gate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var bootstrap = BootstrapFactory.CreateBootstrap();
            if (!bootstrap.Initialize())
            {
                throw new Exception("网关服务器初始化失败");
            }
            var result = bootstrap.Start();
            if (result == StartResult.Failed)
            {
                throw new Exception("网关服务器启动失败");
            }
            Console.WriteLine("网关服务器启动成功");
            while (true)
            {
                if (Console.ReadLine() == "quit")
                {
                    break;
                }
                else
                {
                    Console.WriteLine(bootstrap.AppServers.First().SessionCount);
                }
            }

            bootstrap.Stop();

            Console.WriteLine("byebye");
        }
    }
}
