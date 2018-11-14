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
                .ForMember(x => x.TransactionsCount, opt => opt.MapFrom(x => 
                    x.TransactedAssets
                        .Where(ta => ta.Transaction != null)
                        .Select(ta => ta.TransactionScriptHash)
                        .Union(x.TransactedAssets.Where(ta => ta.InGlobalTransactionScriptHash != null).Select(ta => ta.InGlobalTransactionScriptHash))
                        .Union(x.TransactedAssets.Where(ta => ta.OutGlobalTransaction != null).Select(ta => ta.OutGlobalTransactionScriptHash))
                        .Distinct()
                        .Count()))
                .ForMember(x => x.AddressesCount, opt => opt.MapFrom(x => x.Balances.Count()))
                .ForMember(x => x.NewAddressesLastMonth, opt => opt.MapFrom(
                    x => x.Balances.Where(b => b.Address.FirstTransactionOn >= DateTime.UtcNow.AddMonths(-1)).Count()))
                .ForMember(x => x.ActiveAddressesLastMonth, opt => opt.MapFrom(
                    x => x.Balances.Where(b => b.Address.LastTransactionOn >= DateTime.UtcNow.AddMonths(-1)).Count()))
                .ReverseMap();
        }
    }
}
