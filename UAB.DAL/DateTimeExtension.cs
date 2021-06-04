using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL
{
    public static class DateTimeExtension
    {
        public static DateTime ToLocalDate(this DateTime utcDateTime, string timeZoneOffset)
        {
            double clientUtcHoursOffset = Convert.ToDouble(timeZoneOffset);
            clientUtcHoursOffset = Math.Round(clientUtcHoursOffset, 2);
            return utcDateTime.AddHours(clientUtcHoursOffset);
        }
        public static DateTime ToUtcDate(this DateTime DateTime, string timeZoneOffset)
        {
            double clientUtcHoursOffset = Convert.ToDouble(timeZoneOffset);
            clientUtcHoursOffset = Math.Round(clientUtcHoursOffset, 2);
            return DateTime.AddHours(-clientUtcHoursOffset);
        }
    }
}
