using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;
using System;

namespace StateOfNeo.Common.Helpers.Filters
{
    public class DateOrderFilter
    {
        public static object GetGroupBy(uint timestamp, UnitOfTime uot)
        {
            return GetGroupBy(timestamp.ToUnixDate(), uot);
        }

        public static object GetGroupBy(long timestamp, UnitOfTime uot)
        {
            return GetGroupBy(timestamp.ToUnixDate(), uot);
        }

        public static object GetGroupBy(DateTime date, UnitOfTime uot)
        {

            if (uot == UnitOfTime.Month)
            {
                return new
                {
                    date.Year,
                    date.Month
                };
            }
            if (uot == UnitOfTime.Day)
            {
                return new
                {
                    date.Year,
                    date.Month,
                    date.Day
                };
            }

            return new
            {
                date.Year,
                date.Month,
                date.Day,
                date.Hour
            };
        }

        public static DateTime GetDateTime(uint timestamp, UnitOfTime uot)
        {
            return GetDateTime(timestamp.ToUnixDate(), uot);
        }

        public static DateTime GetDateTime(long timestamp, UnitOfTime uot)
        {
            return GetDateTime(timestamp.ToUnixDate(), uot);
        }

        public static DateTime GetDateTime(DateTime date, UnitOfTime uot)
        {
            if (uot == UnitOfTime.Month)
            {
                return new DateTime(date.Year, date.Month, 1);
            }
            if (uot == UnitOfTime.Day)
            {
                return new DateTime(date.Year, date.Month, date.Day);
            }
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
        }
    }
}
