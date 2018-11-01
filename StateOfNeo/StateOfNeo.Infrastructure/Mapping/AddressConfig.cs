using AutoMapper;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class AddressConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Address, AddressListViewModel>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => x.PublicAddress))
                .ForMember(x => x.Created, opt => opt.MapFrom(x => x.FirstTransactionOn))
                .ForMember(x => x.Transactions, opt => opt.MapFrom(x => x.OutgoingTransactions.Count()))
                .ForMember(x => x.LastTransactionTime, opt => opt.MapFrom(x => x.OutgoingTransactions.Select(tr => tr.Transaction.Block.Timestamp).OrderByDescending(ts => ts).FirstOrDefault().ToUnixDate()))

                .ReverseMap();

        }
    }
}
