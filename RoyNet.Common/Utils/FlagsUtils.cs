using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RoyNet.Common
{
    public static class FlagsUtils
    {
        public static bool HasFlag(this int n, int flag)
        {
            if (flag > 30)
            {
                throw new OverflowException("flag can't bigger than 64");
            }
            return (n & (1 << flag)) != 0;
        }

        public static int SetFlag(this int n, int flag)
        {
            if (flag > 30)
            {
                throw new OverflowException("flag can't bigger than 64");
            }
            return n | (1 << flag);
        }
    }
}
