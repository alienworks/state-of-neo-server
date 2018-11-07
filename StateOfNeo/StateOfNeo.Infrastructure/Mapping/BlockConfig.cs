using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using StateOfNeo.ViewModels.Block;
using System;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class BlockConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Block, BlockHubViewModel>()
                .ForMember(x => x.TransactionCount, y => y.MapFrom(z => z.Transactions.Count))
                .ReverseMap();

            cfg.CreateMap<Block, BlockDetailsViewModel>()
                .ForMember(x => x.SecondsFromPreviousBlock, y => y.MapFrom(z => z.TimeInSeconds))
                .ReverseMap();

            cfg.CreateMap<Block, BlockListViewModel>()
                .ReverseMap();
        }
    }
}
