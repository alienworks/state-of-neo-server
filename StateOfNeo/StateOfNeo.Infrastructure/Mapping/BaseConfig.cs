using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class BaseConfig
    {
        internal static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<StampedEntity, StampViewModel>().ReverseMap();
        }
    }
}
