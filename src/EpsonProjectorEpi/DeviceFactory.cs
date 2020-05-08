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
using EpsonProjectorEpi.States;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi
{
    public class DeviceFactory
    {
        public static void LoadPlugin()
        {
            PepperDash.Essentials.Core.DeviceFactory.AddFactoryForType("epsonprojector", config =>
                {
                    return new EpsonProjector(config).BuildInputs();
                });
        }
    }
}