using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiscUtil.Conversion;
using ProtoBuf;
using RoyNet.GameServer.Entity;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGetToken_Click(object sender, EventArgs e)
        {
            string username = textBoxUserName.Text;
            string password = textBoxPassword.Text;
            WebRequest request = WebRequest.Create("http://127.0.0.1:8080/login/" + username+"/"+password);
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            if (stream != null  && stream.CanRead)
            {
                byte[] buffer = new byte[128];
                int length = stream.Read(buffer, 0, buffer.Length);
                var result = Json.JsonParser.Deserialize(Encoding.UTF8.GetString(buffer, 0, length));
                if (result.Result == "OK")
                {
                    textBoxToken.Text = result.Token;
                }
                else
                {
                    MessageBox.Show(result.Result);
                }
            }

        }

        private void buttonEnter_Click(object sender, EventArgs e)
        {
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 2020);
            var stream = client.GetStream();
            Rece(stream);
            if (stream.CanWrite)
            {
                var converter = EndianBitConverter.Big;
                string token = textBoxToken.Text;
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
                stream.Write(data, 0, data.Length);
            }
        }

        void Launch(NetworkStream stream)
        {
            //TcpClient client = new TcpClient();
            //client.Connect("127.0.0.1", 2020);
            //var stream = client.GetStream();
            Rece(stream);
            while (stream.CanWrite)
            {
                string text = Console.ReadLine();

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
                stream.Write(data, 0, data.Length);
            }
            //2个没啥用，1个指定服务器，1个gatecmd，2个length，4个gamecmd
        }

        void Rece(NetworkStream stream)
        {
            byte[] recedata = new byte[1024];
            stream.BeginRead(recedata, 0, recedata.Length, (a) =>
            {
                int receLength = stream.EndRead(a);
                if (receLength == 1)
                {
                    Console.WriteLine("登录成功！");
                    Launch(stream);
                }
                else
                {
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
                }
            }, null);
        }
    }
}
