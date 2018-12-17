using Neo;
using Neo.Cryptography;
using StateOfNeo.Common.Enums;
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

        public static bool IsOlderThan(this long timestamp, UnitOfTime unitOfTime)
        {
            var date = ToUnixDate(timestamp);
            var now = DateTime.UtcNow;
            if (unitOfTime == UnitOfTime.Hour)
            {
                return date.AddHours(1) < now;
            }
            else if (unitOfTime == UnitOfTime.Day)
            {
                return date.AddDays(1) < now;
            }
            else if (unitOfTime == UnitOfTime.Month)
            {
                return date.AddMonths(1) < now;
            }

            return false;
        }
        
        //public static string ToAddress(this UInt160 scriptHash)
        //{
        //    byte[] data = new byte[21];
        //    //data[0] = Settings.Default.AddressVersion;
        //    Buffer.BlockCopy(scriptHash.ToArray(), 0, data, 1, 20);
        //    return data.Base58CheckEncode();
        //}
    }
}
