using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Common.Extensions
{
    public static class NumberExtensions
    {
        public static DateTime ToUnixDate(this long timestamp) =>
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();

        public static DateTime ToUnixDate(this uint timestamp) =>
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();
    }
}
