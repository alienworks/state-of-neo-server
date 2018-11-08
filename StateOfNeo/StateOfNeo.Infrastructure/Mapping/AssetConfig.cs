using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class AssetConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Asset, AssetListViewModel>()
                .ForMember(x => x.TotalSupply, opt => opt.MapFrom(x => x.MaxSupply))
                .ReverseMap();

            cfg.CreateMap<Asset, AssetDetailsViewModel>()
                .ForMember(x => x.TotalSupply, opt => opt.MapFrom(x => x.MaxSupply))
                .ForMember(x => x.TransactionsCount, opt => opt.MapFrom(x => x.TransactedAssets.Count))
                .ForMember(x => x.AddressesCount, opt => opt.MapFrom(x => x.TransactedAssets.Select(ta => ta.FromAddressPublicAddress).Union(x.TransactedAssets.Select(ta => ta.ToAddressPublicAddress)).Distinct().Count()))
                .ReverseMap();
        }
    }
}
