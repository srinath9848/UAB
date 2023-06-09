﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAB
{
    public static class ExtensionMethods
    {
        public static DateTime ToLocalDate(this DateTime dateTime)
        {
            DateTime dt = TimeZoneInfo.ConvertTimeToUtc(dateTime);
            return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.Local);
        }
    }
}
