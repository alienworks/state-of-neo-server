using System;
using System.Text;
using System.Text.RegularExpressions;

namespace StateOfNeo.Common
{
    public static class StringExtensions
    {
        public static string ToMatchedIp(this string ipString)
        {
            Match match = Regex.Match(ipString, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            if (match.Success)
            {
                return match.Value;
            }
            return ipString;
        }

        public static string HexStringToString(this string hexString)
        {
            if (hexString == null || (hexString.Length & 1) == 1)
            {
                throw new ArgumentException();
            }

            var sb = new StringBuilder();
            for (var i = 0; i < hexString.Length; i += 2)
            {
                var hexChar = hexString.Substring(i, 2);
                sb.Append((char)Convert.ToByte(hexChar, 16));
            }

            return sb.ToString();
        }
    }
}
