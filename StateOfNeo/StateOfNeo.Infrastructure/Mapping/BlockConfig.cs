using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Block;
using System;
using System.Linq;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class BlockConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Block, HeaderStatsViewModel>()
                .ForMember(x => x.TransactionCount, y => y.MapFrom(z => z.Transactions.Count))
                .ReverseMap();

            cfg.CreateMap<Block, BlockDetailsViewModel>()
                .ForMember(x => x.SecondsFromPreviousBlock, y => y.MapFrom(z => z.TimeInSeconds))
                .ReverseMap();

            cfg.CreateMap<Block, BlockListViewModel>()
                .ForMember(x => x.CollectedFees, opt => opt.MapFrom(x => x.Transactions.Sum(t => t.NetworkFee)))
                .ReverseMap();
        }
    }
}
