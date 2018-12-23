using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;

namespace StateOfNeo.ViewModels.Chart
{
    public class ChartFilterViewModel
    {
        public DateTime? StartDate { get; set; }

        public long? StartStamp { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(6, 36)]
        public int EndPeriod { get; set; } = 6;

        public UnitOfTime UnitOfTime { get; set; } = UnitOfTime.Hour;

        public DateTime GetEndPeriod()
        {
            if (this.UnitOfTime == UnitOfTime.Hour) return new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, this.StartDate.Value.Day, this.StartDate.Value.Hour, 0, 0).AddHours(-this.EndPeriod);
            if (this.UnitOfTime == UnitOfTime.Day) return new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, this.StartDate.Value.Day).AddDays(-this.EndPeriod);
            if (this.UnitOfTime == UnitOfTime.Month) return new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, 1).AddMonths(-this.EndPeriod);
            return new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, this.StartDate.Value.Day).AddDays(-this.EndPeriod);
        }

        public IEnumerable<long> GetPeriodStamps()
        {
            List<long> periods = new List<long>();

            var lastPeriod = default(DateTime);
            if (this.UnitOfTime == UnitOfTime.Hour)
            {
                lastPeriod = new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, this.StartDate.Value.Day, this.StartDate.Value.Hour, 0, 0);
            }
            else if (this.UnitOfTime == UnitOfTime.Day)
            {
                lastPeriod = new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, this.StartDate.Value.Day);
            }
            else if (this.UnitOfTime == UnitOfTime.Month)
            {
                lastPeriod = new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, 1);
            }

            periods.Add(((DateTimeOffset)lastPeriod).ToUnixTimeSeconds());

            for (int i = 1; i <= this.EndPeriod - 1; i++)
            {
                var end = default(DateTime);
                if (this.UnitOfTime == UnitOfTime.Hour)
                {
                    end = new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, this.StartDate.Value.Day, this.StartDate.Value.Hour, 0, 0).AddHours(-i);
                }
                else if (this.UnitOfTime == UnitOfTime.Day)
                {
                    end = new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, this.StartDate.Value.Day).AddDays(-i);
                }
                else if (this.UnitOfTime == UnitOfTime.Month)
                {
                    end = new DateTime(this.StartDate.Value.Year, this.StartDate.Value.Month, 1).AddMonths(-i);
                }

                periods.Add(((DateTimeOffset)end).ToUnixTimeSeconds());
            }

            return periods;
        }

        public long LatestTimestamp => this.StartDate.HasValue ? this.GetEndPeriod().ToUnixTimestamp() : 0;
    }
}
