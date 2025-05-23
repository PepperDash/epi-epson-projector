﻿using Newtonsoft.Json;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

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
    }
}