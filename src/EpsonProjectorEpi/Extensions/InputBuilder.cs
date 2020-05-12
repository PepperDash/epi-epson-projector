using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi.Extensions
{
    public static class EpsonExtensions
    {
        public static IKeyed BuildInputs(this IRoutingInputs device)
        {
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