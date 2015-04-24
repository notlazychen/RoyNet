using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MiscUtil.Conversion;
using ProtoBuf;
using RoyNet.GameServer.Entity;

namespace Roynet.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 登录服测试
            //DateTime startTime = DateTime.Now;
            //int cntTotal = 0;
            //int failed = 0;
            //int success = 0;
            //while (true)
            //{
            //    string msg;
            //    if (HttpHelper.TryGet(new Uri("http://127.0.0.1:8080/cr/123"), out msg))
            //    {
            //        success++;
            //        //Console.WriteLine(msg);
            //    }
            //    else
            //    {
            //        failed ++;
            //        //Console.WriteLine("连接失败");
            //    }
            //    cntTotal++;
            //    DateTime endTime = DateTime.Now;
            //    if ((endTime - startTime).TotalSeconds > 1)
            //    {
            //        startTime = endTime;
            //        Console.WriteLine("共获取{0}次,成功{1}次，失败{2}次", cntTotal, success, failed);
            //    }
            //}
            #endregion

            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 2020);
            var stream = client.GetStream();
            Rece(stream);
            while (stream.CanWrite)
            {
                string text = Console.ReadLine();

                var converter = EndianBitConverter.Big;

                var msm = new MemoryStream();
                Serializer.Serialize(msm, new Chat_Send(){Text = text});
                var body = msm.ToArray();
                int length = body.Length;

                byte[] data = new byte[length+10];
                int offset = 0;
                data[3] = 0x02; //cmd
                offset += 4;
                converter.CopyBytes((ushort)(length + 4), data, offset);
                offset += 2;
                converter.CopyBytes((int)CMD_Chat.Send, data, offset);
                offset += 4;
                Buffer.BlockCopy(body, 0, data, offset, length);
                stream.Write(data, 0, data.Length);
            }
            //2个没啥用，1个指定服务器，1个gatecmd，2个length，4个gamecmd
        }

        static void Rece(NetworkStream stream)
        {
            byte[] recedata = new byte[1024];
            stream.BeginRead(recedata, 0, recedata.Length, (a) =>
            {
                int receLength = stream.EndRead(a);
                var converter = EndianBitConverter.Big;
                int offset = 0;
                while (offset < receLength + 2)
                {
                    int length = converter.ToInt16(recedata, offset);
                    offset += 2;
                    int cmd = converter.ToInt32(recedata, offset);
                    offset += 4;
                    using (var receMs = new MemoryStream())
                    {
                        receMs.Write(recedata, offset, length - 4);
                        receMs.Position = 0;
                        var package = Serializer.Deserialize<Chat_Send>(receMs);
                        Console.WriteLine("{0}", package.Text);
                        offset += length;
                    }
                }
                Rece(stream);
            }, null);
        }
    }
}
