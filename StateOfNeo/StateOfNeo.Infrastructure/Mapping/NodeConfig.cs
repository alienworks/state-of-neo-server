using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using System.Linq;

namespace StateOfNeo.Infrastructure.Mapping
{
    internal class NodeConfig
    {
        public static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Node, NodeViewModel>()
                .ForMember(x => x.Ips,
                    y => y.MapFrom(
                        z => z.NodeAddresses.Any()
                            ? z.NodeAddresses.Select(na => na.Ip).ToArray()
                            : new string[] { "" }));

            cfg.CreateMap<Node, NodeDetailsViewModel>();
            cfg.CreateMap<Node, NodeViewModel>();
            cfg.CreateMap<Node, NodeListViewModel>().ReverseMap();
        }
    }
}
