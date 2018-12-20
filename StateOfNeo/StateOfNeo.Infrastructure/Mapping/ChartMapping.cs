using AutoMapper;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Chart;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class ChartMapping
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ChartEntry, ChartStatsViewModel>()
                .ForMember(x => x.StartDate, y => y.MapFrom(z => z.TimeStamp.ToUnixDate()));
        }
    }
}
