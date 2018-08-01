using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyNet.Common
{
    public static class DateTimeEx
    {
        /// <summary>
        /// 获取Unix时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns>毫秒</returns>
        public static long ToUnixTime(this DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }

        private readonly static DateTime UnixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// 获取Unix时间
        /// </summary>
        /// <param name="time">毫秒</param>
        public static DateTime ConvertToUnixTime(this long time)
        {
            return UnixTime.AddMilliseconds(time);
        }

        /// <summary>
        /// 中国习惯同一周, 周日为一周之末
        /// </summary>
        public static bool IsSameWeek(this DateTime dt1, DateTime dt2)
        {
            int delta1 = dt1.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt1.DayOfWeek;
            int delta2 = dt2.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt2.DayOfWeek;
            DateTime temp1 = dt1.AddDays(-delta1).Date;
            DateTime temp2 = dt2.AddDays(-delta2).Date;
            bool result = temp1 == temp2;
            return result;
        }

        /// <summary>
        /// 中国习惯的周几, 周日为7
        /// </summary>
        public static int GetWeekDay(this DateTime dt)
        {
            return dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek;
        }

        public static bool IsPass(this DateTime now, DateTime lastTime, int[] hours)
        {
            foreach (int hour in hours)
            {
                DateTime time = hour == 24 ? DateTime.Today : DateTime.Today.AddHours(hour);
                if (lastTime <= time && now >= time)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
