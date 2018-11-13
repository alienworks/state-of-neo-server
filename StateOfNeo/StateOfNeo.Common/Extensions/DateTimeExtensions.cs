using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimestamp(this DateTime date) =>
            (long)date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}
