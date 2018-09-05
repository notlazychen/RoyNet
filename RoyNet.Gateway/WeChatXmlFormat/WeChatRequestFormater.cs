using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RoyNet.Gateway
{
    public class WeChatRequestConvert
    {
//        public const string MsgFormat = @"<xml><ToUserName><![CDATA[gh_3a0c51b1684a]]></ToUserName>
//<FromUserName><![CDATA[oFtqWuCE9p-YhZC-NUCblRtW3_6I]]></FromUserName>
//<CreateTime>1534922313</CreateTime>
//<MsgType><![CDATA[text]]></MsgType>
//<Content><![CDATA[,,ddd]]></Content>
//<MsgId>6592441136693865215</MsgId>
//</xml>";

        public static T Deserialize<T>(string body)
        {
            using (var reader = new StringReader(body))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                var resultingMessage = (T)serializer.Deserialize(reader);
                return resultingMessage;
            }
        }

        public static T Deserialize<T>(Stream body)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            var resultingMessage = (T)serializer.Deserialize(body);
            return resultingMessage;
        }

        //public static string Serialize<T>(T obj)
        //{
        //    using (var writer = new StringWriter())
        //    {
        //        XmlSerializer serializer = new XmlSerializer(typeof(T));
        //        serializer.Serialize(writer, obj);
        //        return writer.GetStringBuilder().ToString();
        //    }
        //}
    }


    public class WeChatEventRequestModel : WechatMessageModel
    {
        [XmlElement]
        public string Event { get; set; }
    }


    public class WeChatMessageRequestModel : WechatMessageModel
    {
        [XmlElement]
        public string Content { get; set; }
        [XmlElement]
        public string MsgId { get; set; }
    }

    [XmlRoot("xml")]
    public class WechatMessageModel
    {
        [XmlElement]
        public string ToUserName { get; set; }
        [XmlElement]
        public string FromUserName { get; set; }
        [XmlElement]
        public int CreateTime { get; set; }
        [XmlElement]
        public string MsgType { get; set; }
    }
}
