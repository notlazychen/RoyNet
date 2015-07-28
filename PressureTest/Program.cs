using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MiscUtil.Conversion;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using RoyNet.Server.GameEngine.Entity;

namespace PressureTest
{
    class Program
    {
        private static string token;
        private static NetworkStream _stream;
        private static TcpClient _client;
        static void Main(string[] args)
        {
            string username = "test1";
            string password = "123";
            WebRequest request = WebRequest.Create("http://127.0.0.1:2020/login/" + username + "/" + password);
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            if (stream != null && stream.CanRead)
            {
                byte[] buffer = new byte[10240];
                int length = stream.Read(buffer, 0, buffer.Length);

                dynamic result = JObject.Parse(Encoding.UTF8.GetString(buffer, 0, length));
                if (result["result"].ToString() == "OK")
                {
                    token = result["token"].ToString();
                    Server server = new Server()
                    {
                        //DestID = s.destID,
                        IP = "127.0.0.1",
                        Name = "测试",
                        Port = 2021
                    };
                    EnterGame(server);
                }
                else
                {
                    Console.WriteLine(result["result"].ToString());
                }
            }

            Console.WriteLine("over");
            int times = 1;
            while (true)
            {
                //string text = times++.ToString();
                //times *= 2;
                SendChat(times++.ToString());
                Thread.Sleep(10);
            }
        }

        private static void EnterGame(Server server)
        {
            if (_stream == null || !_stream.CanWrite)
            {
                if (server == null)
                    return;
                _client = new TcpClient();
                _client.Connect(server.IP, server.Port);
                _stream = _client.GetStream();
                Rece(_stream);
            }

            var converter = EndianBitConverter.Big;
            byte[] body = Encoding.UTF8.GetBytes(token);
            int length = body.Length;

            byte[] data = new byte[length + 10];
            int offset = 0;
            data[3] = 0x01; //cmd
            offset += 4;
            converter.CopyBytes((ushort)(length + 4), data, offset);
            offset += 2;
            converter.CopyBytes((int)CMD_Chat.Send, data, offset);
            offset += 4;
            Buffer.BlockCopy(body, 0, data, offset, length);
            _stream.Write(data, 0, data.Length);
        }

        static void Rece(NetworkStream stream)
        {
            byte[] recedata = new byte[1024];
            stream.BeginRead(recedata, 0, recedata.Length, (a) =>
            {
                int receLength = stream.EndRead(a);
                if (receLength == 0)
                {
                    Log("服务器关闭了连接。可能是顶号。");
                    _client.Close();
                }
                else if (receLength == 1)
                {
                    Log("登录成功！");
                    Rece(stream);
                }
                else
                {
                    var converter = EndianBitConverter.Big;
                    int offset = 0;
                    while (offset < receLength)
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

                            Log(package.Text.ToString());
                            offset += (length - 4);
                        }
                    }
                    Rece(stream);
                }
            }, null);
        }

        static void Log(string text)
        {
            Console.WriteLine(text);
        }

        static void SendChat(string text)
        {
            if (_client.Connected && _stream.CanWrite)
            {
                var converter = EndianBitConverter.Big;
                var msm = new MemoryStream();
                Serializer.Serialize(msm, new Chat_Send() { Text = text });
                var body = msm.ToArray();
                int length = body.Length;

                byte[] data = new byte[length + 10];
                int offset = 0;
                data[3] = 0x02; //cmd
                offset += 4;
                converter.CopyBytes((ushort)(length + 4), data, offset);
                offset += 2;
                converter.CopyBytes((int)CMD_Chat.Send, data, offset);
                offset += 4;
                Buffer.BlockCopy(body, 0, data, offset, length);
                _stream.Write(data, 0, data.Length);
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
