using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Roynet.Test
{
    public static class HttpHelper
    {
        //private static DateTime _lastRequestTime = DateTime.MinValue;
        public static bool TryGet(Uri uri, out string responseMsg)
        {
            responseMsg = null;
            WebRequest request = WebRequest.Create(new Uri("http://127.0.0.1:8080/cr/123"));
            request.Timeout = 1000;
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            byte[] buffer = new byte[1024];
            if (stream != null && stream.CanRead)
            {
                int ss = stream.Read(buffer, 0, buffer.Length);
                responseMsg = Encoding.UTF8.GetString(buffer, 0, ss);
                return true;
            }
            return false;
        }
    }

}
