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
            //.Where(x =>
            //        x.Assets.Any(a => a.AssetHash == asset)
            //        || x.GlobalIncomingAssets.Any(a => a.AssetHash == asset)
            //        || x.GlobalIncomingAssets.Any(a => a.AssetHash == asset))
            cfg.CreateMap<Asset, AssetDetailsViewModel>()
                .ForMember(x => x.TotalSupply, opt => opt.MapFrom(x => x.MaxSupply))
                .ForMember(x => x.TransactionsCount1, opt => opt.MapFrom(x => x.TransactionsCount))
                .ForMember(x => x.TransactionsCount2, opt => opt.MapFrom(x => x.AddressesInTransactions.Count))
                .ForMember(x => x.TransactionsCount3, opt => opt.MapFrom(x =>
                    x.TransactedAssets
                        .Where(ta => ta.Transaction != null)
                        .Select(ta => ta.TransactionHash)
                        .Union(x.TransactedAssets.Where(ta => ta.InGlobalTransactionHash != null).Select(ta => ta.InGlobalTransactionHash))
                        .Union(x.TransactedAssets.Where(ta => ta.OutGlobalTransaction != null).Select(ta => ta.OutGlobalTransactionHash))
                        .Distinct()
                        .Count()))

                .ForMember(x => x.AddressesCount, opt => opt.MapFrom(x => x.Balances.Count()))
                .ForMember(x => x.NewAddressesLastMonth, opt => opt.MapFrom(
                    x => x.Balances.Count(b => b.Address.FirstTransactionOn >= DateTime.UtcNow.AddMonths(-1))))
                .ForMember(x => x.ActiveAddressesLastMonth, opt => opt.MapFrom(
                    x => x.Balances.Count(b => b.Address.LastTransactionOn >= DateTime.UtcNow.AddMonths(-1))))
                .ReverseMap();
        }
    }
}
