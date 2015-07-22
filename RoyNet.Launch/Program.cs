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
        static void Main(string[] args)
        {
            Launcher launcher = Launcher.Create();
            launcher.Ignition();
            Console.WriteLine("All servers have been started!");
            while (true)
            {
                string cmd = Console.ReadLine();
                if (cmd == "stop")
                {
                    launcher.Stop();
                    break;
                }
            }
        }
    }
}
