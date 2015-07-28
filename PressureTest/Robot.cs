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
    class Robot
    {
        public static int LoginCount = 0;
        public Robot(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
        private string _token;
        private NetworkStream _stream;
        private TcpClient _client;
        public void Go()
        {
            string username = Name;
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
                    _token = result["token"].ToString();
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

            Thread thread = new Thread(() =>
            {
                int times = 1;
                Random random = new Random();
                while (true)
                {
                    SendChat(times++.ToString());
                    Thread.Sleep(random.Next(1000, 6000));
                } 
            });
            thread.Start();
        }
        void EnterGame(Server server)
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
            byte[] body = Encoding.UTF8.GetBytes(_token);
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

        void Rece(NetworkStream stream)
        {
            byte[] recedata = new byte[1024];
            stream.BeginRead(recedata, 0, recedata.Length, (a) =>
            {
                int receLength = stream.EndRead(a);
                if (receLength == 0)
                {
                    Log(string.Format("[{0}] 服务器关闭了连接。可能是顶号。", Name));
                    _client.Close();
                }
                else if (receLength == 1)
                {
                    Log(string.Format("[{0}] 登录成功！", Name));
                    Interlocked.Increment(ref LoginCount);
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
                            //if ((length - 4) < 0 || (length -4 + offset)> receLength)
                            //{
                            //    continue;
                            //}
                            receMs.Write(recedata, offset, length - 4);
                            receMs.Position = 0;
                            var package = Serializer.Deserialize<Chat_Send>(receMs);

                            //Log(package.Text.ToString());
                            offset += (length - 4);
                        }
                    }
                    Rece(stream);
                }
            }, null);
        }

        void Log(string text)
        {
            Console.WriteLine(text);
        }

        void SendChat(string text)
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
}
