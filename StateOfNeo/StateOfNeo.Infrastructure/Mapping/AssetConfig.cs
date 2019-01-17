using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class AssetConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Asset, AssetListViewModel>()
                .ForMember(x => x.TotalSupply, opt => 
                    opt.MapFrom(x => x.MaxSupply.HasValue ? new BigInteger(x.MaxSupply.Value) : new BigInteger(0)))
                .ReverseMap();
            //.Where(x =>
            //        x.Assets.Any(a => a.AssetHash == asset)
            //        || x.GlobalIncomingAssets.Any(a => a.AssetHash == asset)
            //        || x.GlobalIncomingAssets.Any(a => a.AssetHash == asset))
            cfg.CreateMap<Asset, AssetDetailsViewModel>()
                .ForMember(x => x.TotalSupply, opt => opt.MapFrom(x => x.MaxSupply))
                .ReverseMap();
        }
    }
}
