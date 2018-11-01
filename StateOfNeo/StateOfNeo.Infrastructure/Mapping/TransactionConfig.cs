using AutoMapper;
using StateOfNeo.ViewModels.Transaction;
using StateOfNeo.Data.Models.Transactions;
using StateOfNeo.Common.Extensions;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class TransactionConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Transaction, TransactionListViewModel>()
                .ForMember(x => x.FinalizedAt, opt => opt.MapFrom(x => x.Block.Timestamp.ToUnixDate()))
                .ForMember(x => x.Timestamp, opt => opt.MapFrom(x => x.Block.Timestamp))
                .ForMember(x => x.Hash, opt => opt.MapFrom(x => x.ScriptHash))
                .ReverseMap();

            cfg.CreateMap<Transaction, TransactionDetailsViewModel>()
                .ForMember(x => x.FinalizedAt, opt => opt.MapFrom(x => x.Block.Timestamp.ToUnixDate()))
                .ForMember(x => x.Hash, opt => opt.MapFrom(x => x.ScriptHash))
                .ReverseMap();

            cfg.CreateMap<TransactedAsset, TransactedAssetViewModel>()
                .ForMember(x => x.FromAddress, opt => opt.MapFrom(x => x.FromAddressPublicAddress))
                .ForMember(x => x.ToAddress, opt => opt.MapFrom(x => x.ToAddressPublicAddress))
                .ReverseMap();

            cfg.CreateMap<TransactionAttribute, TransactionAttributeViewModel>()
                .ReverseMap();

            cfg.CreateMap<TransactionWitness, TransactionWitnessViewModel>()
                .ReverseMap();

        }
    }
}
