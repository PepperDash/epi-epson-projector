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
    }
}