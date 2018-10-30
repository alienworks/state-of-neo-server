using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Asset;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class AssetConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Asset, AssetListViewModel>()
                    .ReverseMap();

            cfg.CreateMap<Asset, AssetDetailsViewModel>()
                    .ReverseMap();
        }
    }
}
