using System;
using StateOfNeo.Common.Enums;

namespace StateOfNeo.ViewModels.Chart
{
    public class ChartFilterViewModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public UnitOfTime UnitOfTime { get; set; }
    }
}
