using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace RoyNet.GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Server gameServer = new Server("ipc://game1"))
            {
                Console.WriteLine("正在开启服务器");
                gameServer.Open();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("服务器开启成功，输入quit退出");
                Console.ForegroundColor = ConsoleColor.White;
                while (true)
                {
                    string cmd = Console.ReadLine();
                    if (cmd == "quit")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("unknown command! ");
                    }
                }
                gameServer.Close();
                Console.WriteLine("服务器关闭，按回车退出");
            }
        }
    }
}
