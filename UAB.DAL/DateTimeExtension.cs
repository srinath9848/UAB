using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL
{
    public static class DateTimeExtension
    {
        public static DateTime ToLocalDate(this DateTime dateTime)
        {
            DateTime dt = TimeZoneInfo.ConvertTimeToUtc(dateTime);
            return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.Local);
        }
    }
}
