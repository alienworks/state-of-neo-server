using System;
using System.ComponentModel.DataAnnotations;
using StateOfNeo.Common.Enums;
using StateOfNeo.Common.Extensions;

namespace StateOfNeo.ViewModels.Chart
{
    public class ChartFilterViewModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(6, 36)]
        public int EndPeriod { get; set; } = 6;

        public UnitOfTime UnitOfTime { get; set; } = UnitOfTime.Hour;

        public DateTime GetEndPeriod()
        {
            if (this.UnitOfTime == UnitOfTime.Hour) return this.StartDate.Value.AddHours(-this.EndPeriod);
            if (this.UnitOfTime == UnitOfTime.Day) return this.StartDate.Value.AddDays(-this.EndPeriod);
            if (this.UnitOfTime == UnitOfTime.Month) return this.StartDate.Value.AddMonths(-this.EndPeriod);
            return this.StartDate.Value.AddDays(-this.EndPeriod);
        }

        public long LatestTimestamp => this.StartDate.HasValue ? this.GetEndPeriod().ToUnixTimestamp() : 0;
    }
}
