using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoyNet.Gateway.Models;

namespace RoyNet.Gateway.Controllers
{
    public class GameController : Controller
    {
        IConfiguration _configuration { get; }
        ILogger<GameController> _logger { get; set; }

        public GameController(IConfiguration configuration, ILogger<GameController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult Babel()
        {


            string domain = _configuration["urls"];
            string appid = _configuration.GetValue<string>("WeAppId");
            string redirect_uri = $"{domain}/game/babel";
            redirect_uri = HttpUtility.UrlEncode(redirect_uri);
            string url = $"https://open.weixin.qq.com/connect/oauth2/authorize?redirect_uri={redirect_uri}&appid={appid}&response_type=code&scope=snsapi_userinfo&state=1#wechat_redirect";
            return Json(url);
        }
        public async Task<IActionResult> Code(string code, string state)
        {
            var user = HttpContext.GetUser();
            var app = _appMap[user.app];
            if (app == null)
            {
                return NotFound();
            }
            string appid = app.WeAppId;//Configuration.GetValue<string>("WeAppId");
            string appsecret = app.WeAppSecret;//Configuration.GetValue<string>("WeAppSecret");
            string url = $"https://api.weixin.qq.com/";

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(url);
            string result = await client.GetStringAsync($"/sns/oauth2/access_token?appid={appid}&secret={appsecret}&code={code}&grant_type=authorization_code");
            _logger.LogDebug($"微信AccessToken返回结果:{result}");
            //{ "access_token":"ACCESS_TOKEN","expires_in":7200,"refresh_token":"REFRESH_TOKEN","openid":"OPENID","scope":"SCOPE" }
            if (string.IsNullOrEmpty(result))
            {
                return StatusCode(406);
            }
            //{ "access_token":"12_TSd2ZXp9beDOIKYPNrNK2_n3hI_N9scEC9VNkRMjygyVzQsQ5Y6BjyWmxgRpxwVstwpCnlRr6S3oquCZSOGVbiWLJxilVDDVf-rofMZ745Y",
            // "expires_in":7200,
            // "refresh_token":"12_cnM6OFFZduB-2Npwp4B76hN75_9-ppg2zpWxTCSjuIn9d4uQpUaL2S4z8gcFZ-JNazbFkcPIRHT1KACd8rgEZHlGPCidwNM5aPAIwd1baxE",
            // "openid":"orEfQ0ndJLkwAcJ9KR2Xsv8UmQGA",
            // "scope":"snsapi_userinfo"}
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
                    user.we_open_id = jsonUserInfo.openid;
                    user.we_nickname = jsonUserInfo.nickname;
                    using (var conn = _dbMap.BuildDbConnection(user.app))
                    {
                        conn.Execute("update t_user_info set we_open_id=@we_open_id,we_nickname=@we_nickname where gameid=@gameid", user);
                    }
                    HttpContext.SignIn(user);
                    //记录日志
                    using (var conn = _dbMap.BuildDbConnection(user.app))
                    {
                        DbLogger.LogOperation(conn, $"成功绑定微信|OpenId:{user.we_open_id},NickName:{user.we_nickname}", user.gameid, user.nickname, user.we_open_id, user.we_nickname);
                    }
                    return Redirect($"/{user.app}/index.html#/present");
                    //return Json(new OperateResult { Status = 1, Msg = $"你好{jsonUserInfo.nickname}, 微信绑定成功" });
                }
            }
            return Json(new OperateResult { Status = 0, Msg = $"微信绑定失败" });
        }
    }
}
