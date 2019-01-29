using StateOfNeo.ViewModels.Chart;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace StateOfNeo.Services
{
    public interface INodeService
    {
        T Get<T>(int id);
        IEnumerable<T> GetNodes<T>();
        Task<IPagedList<T>> GetPage<T>(int page = 1, int pageSize = 10);
        IEnumerable<ChartStatsViewModel> LatencyChart(ChartFilterViewModel filter, int nodeId);
        IEnumerable<ChartStatsViewModel> PeersChart(ChartFilterViewModel filter, int nodeId);
        IEnumerable<ChartStatsViewModel> NodeTypesChart();

        Task<bool> GetWsStatusAsync(int nodeId);
    }
}
