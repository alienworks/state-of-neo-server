using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class SmartContractConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SmartContract, SmartContractListViewModel>().ReverseMap();
        }
    }
}
