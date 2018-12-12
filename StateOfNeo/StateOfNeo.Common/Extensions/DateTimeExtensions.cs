using StateOfNeo.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimestamp(this DateTime date) =>
            (long)date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        public static bool IsInSamePeriodAs(this DateTime instance, DateTime date, UnitOfTime unitOfTime)
        {
            if (instance.Year == date.Year
                && instance.Month == date.Month)
            {
                if (unitOfTime == UnitOfTime.Month)
                {
                    return true;
                }
                else if (unitOfTime == UnitOfTime.Day)
                {
                    if (instance.Day == date.Day)
                    {
                        return true;
                    }
                }
                else if (unitOfTime == UnitOfTime.Hour)
                {
                    if (instance.Day == date.Day && instance.Hour == date.Hour)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
