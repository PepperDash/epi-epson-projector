using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Plugins
{
    public class DeviceFactory : EssentialsPluginDeviceFactory<EpsonProjector>
    {
        public DeviceFactory()
        {
            MinimumEssentialsFrameworkVersion = "3.0.0";
            TypeNames = new List<string>() { "epsonProjector" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            try
            {
                var props = PropsConfig.FromDeviceConfig(dc);
                var coms = CommFactory.CreateCommForDevice(dc);
                var device = new EpsonProjector(dc.Key, dc.Name, props, coms);

                return device;
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("EpsonProjector BuildDevice EXCEPTION for '{0}':\r\n{1}", dc.Key, ex);
                Debug.LogMessage(ex, $"EpsonProjector BuildDevice failed for {dc.Key}");
                throw;
            }
        }
    }
}