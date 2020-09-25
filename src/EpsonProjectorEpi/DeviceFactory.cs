using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using EpsonProjectorEpi.Config;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi
{
    public class DeviceFactory : EssentialsPluginDeviceFactory<EpsonProjector>
    {
        public DeviceFactory()
        {
            MinimumEssentialsFrameworkVersion = "1.6.4";
            TypeNames = new List<string>() { "epsonProjector" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var props = PropsConfig.FromDeviceConfig(dc);
            var coms = CommFactory.CreateCommForDevice(dc);
            var device = new EpsonProjector(dc.Key, dc.Name, props, coms);

            return device;
        }
    }
}