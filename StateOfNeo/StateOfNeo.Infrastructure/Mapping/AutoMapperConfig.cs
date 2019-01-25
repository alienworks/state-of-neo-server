using AutoMapper;

namespace StateOfNeo.Infrastructure.Mapping
{
    public class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg => {
                BaseConfig.InitMap(cfg);
                NodeConfig.InitMap(cfg);
                BlockConfig.InitMap(cfg);
                AssetConfig.InitMap(cfg);
                AddressConfig.InitMap(cfg);
                TransactionConfig.InitMap(cfg);
                ChartMapping.InitMap(cfg);
                SmartContractConfig.InitMap(cfg);
                PeerConfig.InitMap(cfg);
            });
        }
    }
}
