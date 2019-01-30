using AutoMapper;
using StateOfNeo.Data.Models.Transactions;
using StateOfNeo.ViewModels.Transaction;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class TransactionConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Transaction, TransactionListViewModel>()
                .ForMember(x => x.Hash, opt => opt.MapFrom(x => x.Hash))
                .ReverseMap();

            cfg.CreateMap<Transaction, TransactionDetailedListViewModel>()
                .ForMember(x => x.Hash, opt => opt.MapFrom(x => x.Hash))
                .ReverseMap();

            cfg.CreateMap<InvocationTransaction, TransactionListViewModel>()
                .ForMember(x => x.Hash, opt => opt.MapFrom(x => x.TransactionHash))
                .ForMember(x => x.Timestamp, opt => opt.MapFrom(x => x.Transaction.Timestamp))
                .ForMember(x => x.Type, opt => opt.MapFrom(x => x.Transaction.Type))
                .ReverseMap();

            cfg.CreateMap<TransactionDetailedListViewModel, TransactionListViewModel>()
                .ReverseMap();

            cfg.CreateMap<Transaction, TransactionDetailsViewModel>()
                .ForMember(x => x.Hash, opt => opt.MapFrom(x => x.Hash))
                .ForMember(x => x.BlockHeight, opt => opt.MapFrom(x => x.Block.Height))
                .ReverseMap();

            cfg.CreateMap<Transaction, TransactionAssetsViewModel>()
                .ReverseMap();

            cfg.CreateMap<TransactedAsset, TransactedAssetViewModel>()
                .ForMember(x => x.FromAddress, opt => opt.MapFrom(x => x.FromAddressPublicAddress))
                .ForMember(x => x.ToAddress, opt => opt.MapFrom(x => x.ToAddressPublicAddress))
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Asset.Name))
                .ReverseMap();

            cfg.CreateMap<TransactionAttribute, TransactionAttributeViewModel>()
                .ReverseMap();

            cfg.CreateMap<TransactionWitness, TransactionWitnessViewModel>()
                .ReverseMap();
        }
    }
}
