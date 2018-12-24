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

        public static DateTime GetPeriodStart(this DateTime date, UnitOfTime unitOfTime)
        {
            if (unitOfTime == UnitOfTime.Hour)
            {
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            }
            else if (unitOfTime == UnitOfTime.Day)
            {
                return new DateTime(date.Year, date.Month, date.Day);
            }
            else if (unitOfTime == UnitOfTime.Month)
            {
                return new DateTime(date.Year, date.Month, 1);
            }

            return default(DateTime);
        }
    }
}
