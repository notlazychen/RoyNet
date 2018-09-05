using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AdventureGrainInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans;
using Rabbit.WeiXin;
using Rabbit.WeiXin.DependencyInjection;
using Rabbit.WeiXin.Handlers;
using Rabbit.WeiXin.Handlers.Impl;
using Rabbit.WeiXin.MvcExtension.Results;
using RoyNet.Common;
using RoyNet.Gateway.Models;

namespace RoyNet.Gateway.Controllers
{
    public class GameController : Controller
    {
        ILogger<GameController> _logger { get; set; }
        IHttpClientFactory _httpClientFactory;
        IClusterClient _client;
        WechatConfig _wechatConfig;
        IWeiXinHandler _weiXinHandler;
        //ISignatureService _signatureService;

        public GameController(ILogger<GameController> logger,
            //ISignatureService signatureService,
            IWeiXinHandler weiXinHandler,
            IOptions<WechatConfig> options, 
            IHttpClientFactory httpClientFactory, IClusterClient client)
        {
            //_signatureService = signatureService;
            _weiXinHandler = weiXinHandler;
            _wechatConfig = options.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _client = client;
        }

        [HttpGet]
        public IActionResult Index(string signature, string timestamp, string nonce, string echostr)
        {
            var signatureService = DefaultDependencyResolver.Instance.GetService<ISignatureService>();
            if (signatureService.Check(signature, timestamp, nonce, _wechatConfig.Token))
                return Content(echostr);
            
            throw new Exception("非法请求。");

            //string token = _wechatConfig.Token;
            ////1）将token、timestamp、nonce三个参数进行字典序排序 
            ////2）将三个参数字符串拼接成一个字符串进行sha1加密
            ////3）开发者获得加密后的字符串可与signature对比，标识该请求来源于微信
            //var canshuarr = new List<string>();
            //canshuarr.Add(timestamp);
            //canshuarr.Add(nonce);
            //canshuarr.Add(token);
            //canshuarr.Sort();
            //var sb = new StringBuilder();
            //foreach (var canshu in canshuarr)
            //{
            //    sb.Append(canshu);
            //}
            //string org = sb.ToString();
            //string des = SecurityUtils.SHA1(org);
            //if (string.Compare(des, signature, true) == 0)
            //{
            //    return Content(echostr);
            //}
            //else
            //{
            //    string msg = $"error: signature:{signature},timestamp:{timestamp},nonce:{nonce},echostr:{echostr} =>org:{org}||des:{des}";
            //    _logger.LogDebug(msg);
            //    return Content(msg);
            //}
        }

        [HttpPost]
        public IActionResult Index()
        {

            string content;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }
            Console.WriteLine(content);
            var context = new HandlerContext(content);

            //设置基本信息。
            context.SetMessageHandlerBaseInfo(new MessageHandlerBaseInfo(
                    _wechatConfig.AppId,
                    _wechatConfig.AppSecret,
                    _wechatConfig.Token));

            _weiXinHandler.Execute(context);

            return new WeiXinResult(context);
        }

        public async Task<IActionResult> Code(string code, string state)
        {
            string appid = _wechatConfig.AppId;
            string appsecret = _wechatConfig.AppSecret;
            string url = $"https://api.weixin.qq.com/";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(url);
            string result = await client.GetStringAsync($"/sns/oauth2/access_token?appid={appid}&secret={appsecret}&code={code}&grant_type=authorization_code");
            _logger.LogDebug($"微信AccessToken 返回结果:{result}");
            if (string.IsNullOrEmpty(result))
            {
                return StatusCode(406);
            }
            var jobj = JsonConvert.DeserializeObject<WeAccessTokenJsonResult>(result);
            if (string.IsNullOrEmpty(jobj.access_token))
            {
                return StatusCode(407);
            }
            string result2 = await client.GetStringAsync($"/sns/userinfo?access_token={jobj.access_token}&openid={jobj.openid}&lang=zh_CN");
            _logger.LogDebug($"微信UserInfo返回结果:{result2}");
            if (!string.IsNullOrEmpty(result2))
            {
                var jsonUserInfo = JsonConvert.DeserializeObject<WeUserInfoJsonResult>(result2);
                if (jsonUserInfo != null && jsonUserInfo.openid != null)
                {
                    //记录日志
                    return Json(new { Status = 1, Msg = $"你好{jsonUserInfo.nickname}, 微信绑定成功" });
                }
            }
            return Json(new { Status = 0, Msg = $"微信绑定失败" });
        }

        public IActionResult Babel()
        {
            //string domain = _configuration["urls"];
            //string appid = _configuration.GetValue<string>("WeAppId");
            //string redirect_uri = $"{domain}/game/babel";
            //redirect_uri = HttpUtility.UrlEncode(redirect_uri);
            //string url = $"https://open.weixin.qq.com/connect/oauth2/authorize?redirect_uri={redirect_uri}&appid={appid}&response_type=code&scope=snsapi_userinfo&state=1#wechat_redirect";
            //return Redirect(url);
            return View();
        }

        public async Task<ContentResult> BabelInput(string text)
        {
            var playerId = Guid.NewGuid().ToString();
            string result = "";
            var player = _client.GetGrain<IPlayerGrain>(playerId);
            string name = await player.Name();
            if (name == null)
            {
                await player.SetName(text);
                var room1 = _client.GetGrain<IRoomGrain>(0);
                player.SetRoomGrain(room1).Wait();
                return Content("hello {0}, welcome to the advanture world! ", text);
            }
            else
            {
                //return Content(player.Play("look").Result);                
                try
                {
                    string command = text;
                    result = await player.Play(command);
                }
                finally
                {
                    player.Die().Wait();
                    result = "Game over!";
                }
                return Content(result);
            }
        }
    }

    class WeAccessTokenJsonResult
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string openid { get; set; }
        public string scope { get; set; }
    }

    class WeUserInfoJsonResult
    {
        public string openid { get; set; }
        public string nickname { get; set; }
        public string sex { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string headimgurl { get; set; }
        public List<string> privilege { get; set; }
        public string unionid { get; set; }
    }
}
