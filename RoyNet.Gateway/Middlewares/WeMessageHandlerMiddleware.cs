using AdventureGrainInterfaces;
using Orleans;
using Rabbit.WeiXin.Handlers;
using Rabbit.WeiXin.Handlers.Impl;
using Rabbit.WeiXin.MP.Messages.Events;
using Rabbit.WeiXin.MP.Messages.Request;
using Rabbit.WeiXin.MP.Messages.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyNet.Gateway
{
    public class WeMessageHandlerMiddleware : MessageHandlerMiddleware
    {
        private IClusterClient _client;
        /// <summary>
        /// 初始化一个新的处理中间件。
        /// </summary>
        /// <param name="next">下一个处理中间件。</param>
        public WeMessageHandlerMiddleware(HandlerMiddleware next, IClusterClient client) : base(next)
        {
            _client = client;
        }

        #region Overrides of MessageHandlerMiddleware

        /// <summary>
        /// 文字类型请求
        /// </summary>
        public override IResponseMessage OnTextRequest(RequestMessageText requestMessage)
        {
            var playerId = requestMessage.FromUserName;
            string result = "";
            var player = _client.GetGrain<IPlayerGrain>(playerId);
            string name = player.Name().Result;
            if (name == null)
            {
                player.SetName(requestMessage.FromUserName).Wait();
                var room1 = _client.GetGrain<IRoomGrain>(0);
                player.SetRoomGrain(room1).Wait();
            }
            //else
            {
                int index = requestMessage.Content.IndexOf(' ');
                string cmd = requestMessage.Content;
                if(index > 0)
                {
                    cmd = requestMessage.Content.Substring(0, index);
                }
                switch (cmd)
                {
                    case "name":
                        {
                            string playername = requestMessage.Content.Substring(5);
                            player.SetName(playername).Wait();
                            return new ResponseMessageText($"你好: {player.Name().Result}");
                        }
                    case "help":
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("--------");
                            sb.AppendLine("命令一栏:");
                            sb.AppendLine("重命名角色: name <名称>");
                            sb.AppendLine("查看四周: look");
                            sb.AppendLine("帮助: help");
                            sb.AppendLine("--------");

                            return new ResponseMessageText($"你好: {player.Name().Result}");
                        }
                    default:
                        string text = requestMessage.Content;
                        //return Content(player.Play("look").Result);                
                        try
                        {
                            string command = text;
                            result = player.Play(command).Result;
                        }
                        finally
                        {
                            player.Die().Wait();
                            result = "Game over!";
                        }
                        return new ResponseMessageText(result);
                }
               
            }
        }

        /// <summary>
        /// Event事件类型请求之subscribe
        /// </summary>
        public override IResponseMessage OnEvent_SubscribeRequest(SubscribeEventMessage requestMessage)
        {
            var playerId = requestMessage.FromUserName;
            var player = _client.GetGrain<IPlayerGrain>(playerId);
            string name = player.Name().Result;
            if (name == null)
            {
                player.SetName(requestMessage.FromUserName).Wait();
                var room1 = _client.GetGrain<IRoomGrain>(0);
                player.SetRoomGrain(room1).Wait();
            }
            var builder = new StringBuilder();
            builder.AppendFormat("你好 {0}, 欢迎进入无限城冒险世界!", requestMessage.FromUserName);
            return new ResponseMessageText(builder.ToString());
        }

        #endregion Overrides of MessageHandlerMiddleware
    }
}
