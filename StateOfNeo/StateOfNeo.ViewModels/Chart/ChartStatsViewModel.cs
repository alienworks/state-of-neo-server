using StateOfNeo.Common.Enums;
using System;

namespace StateOfNeo.ViewModels.Chart
{
    public class ChartStatsViewModel
    {
        public string Label { get; set; }

        public DateTime StartDate { get; set; }

        public long Timestamp { get; set; }

        public UnitOfTime UnitOfTime { get; set; }

        public decimal Value { get; set; }

        public decimal AccumulatedValue { get; set; }
    }
}
