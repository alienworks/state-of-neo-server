using StateOfNeo.ViewModels.Chart;
using System.Collections.Generic;

namespace StateOfNeo.Services.Block
{
    public interface IBlockService
    {
        T Find<T>(string hash);

        T Find<T>(int height);

        int GetHeight(string hash);

        decimal GetAvgTxCountPerBlock();

        double GetAvgBlockTime();

        double GetAvgBlockSize();

        string NextBlockHash(int height);

        IEnumerable<ChartStatsViewModel> GetBlockSizeStats(ChartFilterViewModel filter);

        IEnumerable<ChartStatsViewModel> GetBlockTimeStats(ChartFilterViewModel filter);
    }
}