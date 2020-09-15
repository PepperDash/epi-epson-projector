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

            foreach (var input in ProjectorInput.GetAll())
            {
                Debug.Console(0, device, "Adding Routing input - {0}", input.Name);

                var newInput = new RoutingInputPort(
                        device.Key + "-" + input.Name,
                        eRoutingSignalType.Video,
                        eRoutingPortConnectionType.BackplaneOnly,
                        input,
                        device);


                device.InputPorts.Add(newInput);
            }

            return device;
        }
    }
}