using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using RoyNet.Game.Logic;
using RoyNet.GameServer;

namespace RoyNet.Launch
{
    /// <summary>
    /// 仅仅只是一个启动器，可以作为脚本存在
    /// 如果是通过本脚本启动，切勿直接关闭
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var bootstrap = Bootstrapper.CreateBootstrapper("ipc://game1");

            bootstrap.Start();
            Console.WriteLine("游戏服务器启动成功");

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("--------------");
            Process gatewayProcess = StartGateWay();
            Console.WriteLine("网关服务器启动成功");
            Process loginServerProcess = StartLoginServer();
            Console.WriteLine("登录服务器启动成功");
            Console.WriteLine("--------------");
            
            Console.ForegroundColor = ConsoleColor.White;
            while (true)
            {
                string cmd = Console.ReadLine();
                if (cmd == "quit")
                {
                    break;
                }
                bootstrap.Order(cmd);
                //Console.WriteLine("unknown command!");
            }
            loginServerProcess.Kill();
            Console.WriteLine("登录服务器关闭");

            gatewayProcess.Kill();
            Console.WriteLine("网关服务器关闭");

            bootstrap.Stop();
            Console.WriteLine("游戏服务器关闭");

            Console.WriteLine("服务器关闭，byebye");
        }

        //static Server StartGameServer()
        //{
        //    return gameServer;
        //}

        static Process StartGateWay()
        {
            var ps = Process.GetProcessesByName("RoyNet.GateServer");
            foreach (Process process in ps)
            {
                process.Kill();
            }
            ProcessStartInfo startInfo = new ProcessStartInfo("RoyNet.GateServer.exe")
            {
                WindowStyle = ProcessWindowStyle.Hidden
            };
            return Process.Start(startInfo);
        }

        static Process StartLoginServer()
        {
            var ps = Process.GetProcessesByName("RoyNet.LoginServer");
            foreach (Process process in ps)
            {
                process.Kill();
            }
            ProcessStartInfo startInfo = new ProcessStartInfo("RoyNet.LoginServer.exe")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
                
            };
            return Process.Start(startInfo);
        }
    }
}
