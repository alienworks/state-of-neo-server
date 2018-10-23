using AutoMapper;
using System;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg => {
                NodeConfig.InitMap(cfg);
                BlockConfig.InitMap(cfg);
            });
        }
    }
}
