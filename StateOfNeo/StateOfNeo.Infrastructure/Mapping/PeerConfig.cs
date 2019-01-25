using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using System.Linq;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class PeerConfig
    {
        public static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Peer, PeerViewModel>().ReverseMap();
        }
    }
}
