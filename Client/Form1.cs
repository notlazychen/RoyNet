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
using MiscUtil.Collections.Extensions;
using MiscUtil.Conversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using RoyNet.Server.GameEngine.Entity;

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
            WebRequest request = WebRequest.Create("http://127.0.0.1:2020/login/" + username+"/"+password);
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            if (stream != null  && stream.CanRead)
            {
                byte[] buffer = new byte[1024];
                int length = stream.Read(buffer, 0, buffer.Length);

                dynamic result = JObject.Parse(Encoding.UTF8.GetString(buffer, 0, length));
                if (result["result"].ToString() == "OK")
                {
                    textBoxOutput.Text = result["token"].ToString();
                    List<Server> servers = new List<Server>();
                    //foreach (dynamic s in result.serverList)
                    //{
                        servers.Add(new Server()
                        {
                            //DestID = s.destID,
                            IP = "127.0.0.1",
                            Name = "测试",
                            Port = 2021
                        });
                    //}
                    comboBoxServerList.DataSource = servers;
                }
                else
                {
                    MessageBox.Show(result["result"].ToString());
                }
            }
        }
        
        //2个没啥用，1个指定服务器，1个gatecmd，2个length，4个gamecmd
        void Rece(NetworkStream stream)
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

                            Log(package.Text);
                            offset += length;
                        }
                    }
                    Rece(stream);
                }
            }, null);
        }

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxChat.Text))
            {
                return;
            }

            if (_client.Connected && _stream.CanWrite)
            {
                string text = textBoxChat.Text;
                textBoxChat.Text = string.Empty;

                var converter = EndianBitConverter.Big;
                var msm = new MemoryStream();
                Serializer.Serialize(msm, new Chat_Send() {Text = text});
                var body = msm.ToArray();
                int length = body.Length;

                byte[] data = new byte[length + 10];
                int offset = 0;
                data[3] = 0x02; //cmd
                offset += 4;
                converter.CopyBytes((ushort) (length + 4), data, offset);
                offset += 2;
                converter.CopyBytes((int) CMD_Chat.Send, data, offset);
                offset += 4;
                Buffer.BlockCopy(body, 0, data, offset, length);
                _stream.Write(data, 0, data.Length);
            }
            else
            {
                MessageBox.Show("未连接到服务器！");
            }
        }

        private NetworkStream _stream;
        private TcpClient _client;
        private void btnEnterGame_Click(object sender, EventArgs e)
        {
            if (_stream == null || !_stream.CanWrite)
            {
                Server server = comboBoxServerList.SelectedItem as Server;
                if (server == null)
                    return; 
                _client = new TcpClient();
                _client.Connect(server.IP, server.Port);
                _stream = _client.GetStream();
                Rece(_stream);
            }

            var converter = EndianBitConverter.Big;
            string token = textBoxOutput.Text;
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

        private void Log(string text)
        {
            this.Invoke(new InvokeTextBox(() =>
            {
                textBoxOutput.Text += Environment.NewLine + text;
            }));
        }

        private delegate void InvokeTextBox();

        private void textBoxChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSendChat_Click(null, null);
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
