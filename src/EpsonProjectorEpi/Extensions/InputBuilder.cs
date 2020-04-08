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
        public static IKeyed BuildInputs(this IKeyed device)
        {
            var display = device as IRoutingInputs;
            if (display == null) return device;

            ProjectorInput
                .GetAll()
                .OrderBy(x => x.Value)
                .ToList()
                .ForEach(input =>
                    {
                        display.InputPorts.Add(
                            new RoutingInputPort(
                            display.Key + "-" + input.Name,
                            eRoutingSignalType.Video,
                            eRoutingPortConnectionType.BackplaneOnly,
                            input.Cmd,
                            display));
                    });
            
            return display;
        }
    }
}