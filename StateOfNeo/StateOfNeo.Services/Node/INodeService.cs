using StateOfNeo.ViewModels.Chart;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services
{
    public interface INodeService
    {
        Task<IPagedList<T>> GetPage<T>(int page = 1, int pageSize = 10);
        IEnumerable<ChartStatsViewModel> LatencyChart(ChartFilterViewModel filter, int nodeId);
        IEnumerable<ChartStatsViewModel> PeersChart(ChartFilterViewModel filter, int nodeId);
    }
}
