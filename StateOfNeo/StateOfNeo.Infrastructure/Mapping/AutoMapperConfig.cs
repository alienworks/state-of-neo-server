using AutoMapper;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg => {
                NodeConfig.InitMap(cfg);
                BlockConfig.InitMap(cfg);
                AssetConfig.InitMap(cfg);
                AddressConfig.InitMap(cfg);
                TransactionConfig.InitMap(cfg);
            });
        }
    }
}
