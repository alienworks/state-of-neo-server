using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class BlockConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Block, BlockHubViewModel>()
                .ForMember(x => x.TransactionCount, y => y.MapFrom(z => z.Transactions.Count))
                .ReverseMap();
        }
    }
}
