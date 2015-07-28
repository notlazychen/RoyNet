using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
namespace RoyNet.Launch
{
    /// <summary>
    /// 仅仅只是一个启动器，使用时请将所有配置中依赖的dll放到同目录下
    /// </summary>
    class Program
    {
        private static readonly char[] SEPARATOR = { ' ', ',', '-' };

        static void Main(string[] args)
        {
            Launcher launcher = Launcher.Create();
            launcher.Ignition();
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("************服务器************");
            foreach (ServerBase serverBase in launcher.Servers)
            {
                Console.WriteLine("[{0}] started", serverBase.Name);
            }
            Console.WriteLine("******************************");
            Console.ForegroundColor = defaultColor;
            Console.WriteLine("All servers have been started!");
            Help();

            CommandLoop(launcher);
            Console.WriteLine("byebye");
        }

        static void Help()
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("-命令---参数-----------功能说明-----------");
            Console.WriteLine("stop    serverName/all 停止指定/全部服务器");
            Console.WriteLine("restart serverName/all 重启指定/全部服务器");
            Console.WriteLine("echo                   显示运行中的全部服务器");
            Console.WriteLine("exit                   退出并关闭");
            Console.WriteLine("------------------------------------------");
            Console.ForegroundColor = defaultColor;
        }

        static void CommandLoop(Launcher launcher)
        {
            while (true)
            {
                string[] inputStr = (Console.ReadLine() ?? "").ToLower().Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                if (inputStr.Length == 0)
                    continue;
                string cmd = inputStr[0];
                try
                {
                    switch (cmd)
                    {
                        case "echo":
                            var defaultColor = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("------------------------------");
                            foreach (ServerBase serverBase in launcher.Servers)
                            {
                                Console.WriteLine("[{0}]", serverBase.Name);
                            }
                            Console.WriteLine("------------------------------");
                            Console.ForegroundColor = defaultColor;
                            break;
                        case "restart":
                            OnRestartCommand(launcher, inputStr);
                            break;
                        case "stop":
                            OnStopCommand(launcher, inputStr);
                            break;
                        case "exit":
                            launcher.StopAll();
                            return;
                        default:
                            Console.WriteLine("command is not exist");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        static void OnRestartCommand(Launcher launcher, string[] inputStr)
        {
            if (inputStr.Length != 2)
            {
                throw new ArgumentException("command argument error");
            }
            string param = inputStr[1];
            if (param == "all")
            {
                launcher.StopAll();
                Console.WriteLine("all servers have been stopped");
                launcher.StartAll();
                Console.WriteLine("all servers have been started");
            }
            else
            {
                launcher.Stop(param);
                launcher.Start(param);
                Console.WriteLine("[{0}] restarted", param);
            }
        }

        static void OnStopCommand(Launcher launcher, string[] inputStr)
        {
            if (inputStr.Length != 2)
            {
                throw new ArgumentException("command argument error");
            }
            string param = inputStr[1];
            if (param == "all")
            {
                launcher.StopAll();
                Console.WriteLine("all servers have been stopped");
            }
            else
            {
                launcher.Stop(param);
                Console.WriteLine("[{0}] stopped", param);
            }
        }
    }
}
