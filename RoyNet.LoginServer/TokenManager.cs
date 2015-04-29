using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyNet.LoginServer
{
    public static class TokenManager
    {
        class Token
        {
            public readonly string TokenCode;

            public readonly DateTime LimitTime;

            public Token(DateTime limitTime, string tokenCode)
            {
                TokenCode = tokenCode;
                LimitTime = limitTime;
            }
        }

        private static readonly ConcurrentDictionary<string, Token> _tokensDICT = new ConcurrentDictionary<string, Token>();
        
        public static string CreateToken(string username)
        {
            string token = Guid.NewGuid().ToString();
            _tokensDICT[username] = new Token(DateTime.Now+TimeSpan.FromMinutes(5), token);
            return token;
        }

        public static bool Check(string token)
        {
            KeyValuePair<string, Token> t = _tokensDICT.FirstOrDefault(v => v.Value.TokenCode == token);
            if (t.Equals(default(KeyValuePair<string, Token>)))
            {
                if (t.Value.LimitTime > DateTime.Now)
                {
                    return true;
                }
                Token outt;
                _tokensDICT.TryRemove(t.Key, out outt);
            }
            return false;
        }
    }
}
