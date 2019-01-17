using StateOfNeo.Common.Enums;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Chart;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Services
{
    public interface IAssetService
    {
        T Find<T>(string hash);
        int Count(AssetType[] types);
        int TxCount(AssetType[] types);
        int AddressCount(string hash, UnitOfTime unitOfTime = UnitOfTime.None, bool active = false);

        IEnumerable<ChartStatsViewModel> TokenChart();
    }
}
