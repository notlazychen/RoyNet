using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiscUtil.Conversion;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using RoyNet.Server.GameEngine.Entity;

namespace PressureTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Parallel.For(0, 1000, i =>
            {
                Robot robot = new Robot("test" + i);
                robot.Go();
            });

            while (true)
            {
                Console.ReadLine();
                Console.WriteLine(Robot.LoginCount);
            }
        }

    }

    class Server
    {
        public string Name { get; set; }

        public string IP { get; set; }

        public int Port { get; set; }

        public int DestID { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
