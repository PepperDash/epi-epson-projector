using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using EpsonProjectorEpi.Extensions;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Queries;
using EpsonProjectorEpi.Config;

namespace EpsonProjectorEpi
{
    public class DeviceFactory
    {
        public static void LoadPlugin()
        {
            PepperDash.Essentials.Core.DeviceFactory.AddFactoryForType("epsonprojector", config =>
                {
                    var builder = new DeviceFactory();
                    return builder.BuildDevice(config).BuildInputs();
                });
        }

        public IKeyed BuildDevice(DeviceConfig config)
        {
            var props = PropsConfig.FromDeviceConfig(config);
            var coms = CommFactory.CreateCommForDevice(config);
            var status = new StatusManager(config.Key + "-Status", coms);
            var poll = new PollManager(coms, status);

            var proj = new EpsonProjector(config.Key, config.Name, coms, poll, status);

            if (props.Monitor == null) 
                props.Monitor = new CommunicationMonitorConfig();

            proj.CommunicationMonitor = new GenericCommunicationMonitor(proj, coms, props.Monitor);
            proj.ScreenName = props.ScreenName ?? String.Empty;

            return proj;
        }
    }
}