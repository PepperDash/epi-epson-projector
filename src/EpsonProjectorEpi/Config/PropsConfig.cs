using System.Collections.Generic;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Newtonsoft.Json;

namespace EpsonProjectorEpi.Config
{
    public class PropsConfig
    {
        public static PropsConfig FromDeviceConfig(DeviceConfig config)
        {
            return JsonConvert.DeserializeObject<PropsConfig>(config.Properties.ToString());
        }

        public EssentialsControlPropertiesConfig Control { get; set; }
        public CommunicationMonitorConfig Monitor { get; set; }
        public string ScreenName { get; set; }
        public uint WarmupTime { get; set; }
        public uint CooldownTime { get; set; }
        public Dictionary<string, int> Inputs { get; set; }
    }
}