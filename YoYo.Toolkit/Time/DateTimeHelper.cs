using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoYo.Toolkit.Time
{
    public class DateTimeHelper
    {
        public static DateTime UnixTimestampToDateTimeNoTimeZone(long unixTimestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimestamp).ToLocalTime(); // 转换为本地时间  
            return dateTime;
        }
        public static long DateTimeToUnixTimestampNoTimeZone(DateTime dateTime)
        {
            // 将DateTime（假设为UTC）转换为从1970年1月1日开始的秒数  
            DateTime utcDateTime = dateTime.ToUniversalTime();
            TimeSpan ts = utcDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)ts.TotalMilliseconds;
        }
        /// <summary>
        /// 获取unix时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetUnixTime()
        {
            return ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();
        }
    }
}
