using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Core;

namespace EpsonProjectorEpi
{
    public class PropsConfig
    {
        public static PropsConfig FromDeviceConfig(DeviceConfig config)
        {
            return JsonConvert.DeserializeObject<PropsConfig>(config.Properties.ToString());
        }

        public EssentialsControlPropertiesConfig Control { get; set; }
        public CommunicationMonitorConfig Monitor { get; set; }
        public bool EnableBridgeComms { get; set; }
        
        [JsonProperty("coolingTimeMs")]
        public uint CoolingTimeMs { get; set; }

        [JsonProperty("warmingTimeMs")]
        public uint WarmingTimeMs { get; set; }

        [JsonProperty("activeInputs")]
        public List<ActiveInputs> ActiveInputs { get; set; } 
    }
    
    public class ActiveInputs
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public ActiveInputs()
        {
            Key = string.Empty;
            Name = string.Empty;
        }
    }
}