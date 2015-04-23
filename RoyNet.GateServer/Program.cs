using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;

namespace RoyNet.GateServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var bootstrap = BootstrapFactory.CreateBootstrap();
            if (!bootstrap.Initialize())
            {
                Console.WriteLine("Failed to initialize!");
                Console.ReadLine();
                return;
            }
            var result = bootstrap.Start();
            Console.WriteLine("Start result: {0}!", result);
            if (result == StartResult.Failed)
            {
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Press key 'q' to stop it!");
            while (Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine();
            }
            Console.WriteLine();

            //Stop the appServer
            bootstrap.Stop();
            Console.WriteLine("The server was stopped!");
            Console.ReadLine();
        }
    }
}
