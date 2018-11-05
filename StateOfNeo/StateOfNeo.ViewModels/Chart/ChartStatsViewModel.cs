using StateOfNeo.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.ViewModels.Chart
{
    public class ChartStatsViewModel
    {
        public string Label { get; set; }

        public DateTime StartDate { get; set; }

        public UnitOfTime UnitOfTime { get; set; }

        public decimal Value { get; set; }
    }
}
