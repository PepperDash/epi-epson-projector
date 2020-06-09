using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi
{
    public class EpsonProjectorBridge
    {
        public void LinkToApi(EpsonProjector proj, Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, EpsonProjectorJoinMap joinMap)
        {
            SetupNameFb(proj, trilist, joinMap);

            var isProjFb = new BoolFeedback(() => true);
            isProjFb.LinkInputSig(trilist.BooleanInput[joinMap.IsProjector.JoinNumber]);
            isProjFb.FireUpdate();

            trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, proj.PowerOn);
            trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, proj.PowerOff);
            trilist.SetSigTrueAction(joinMap.MuteOn.JoinNumber, proj.MuteOn);
            trilist.SetSigTrueAction(joinMap.MuteOff.JoinNumber, proj.MuteOff);
            trilist.SetSigTrueAction(joinMap.MuteToggle.JoinNumber, proj.MuteToggle);
            trilist.SetUShortSigAction(joinMap.InputSelectOffset.JoinNumber, x => proj.ExecuteSwitchNumeric(x));

            for (int x = 0; x < joinMap.InputSelectOffset.JoinNumber; x++)
            {
                var joinActual = joinMap.InputSelectOffset.JoinNumber + x;
                var inputActual = x + 1;

                ProjectorInput input;
                if (!ProjectorInput.TryFromValue(inputActual, out input))
                    continue;

                Debug.Console(1, proj, "Linking input:{0} to join {1}", input.Name, joinActual);

                trilist.SetSigTrueAction((uint)joinActual, () => proj.ExecuteSwitch(inputActual));

                var fb = new StringFeedback(() => input.Name);
                fb.LinkInputSig(trilist.StringInput[(uint)joinActual]);
                fb.FireUpdate();
            }

            proj.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn.JoinNumber]);
            proj.IsWarmingUpFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Warming.JoinNumber]);
            proj.IsCoolingDownFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Cooling.JoinNumber]);
            proj.MuteIsOnFb.LinkInputSig(trilist.BooleanInput[joinMap.MuteOn.JoinNumber]);
            proj.MuteIsOffFb.LinkInputSig(trilist.BooleanInput[joinMap.MuteOff.JoinNumber]);
            proj.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            proj.StatusFb.LinkInputSig(trilist.StringInput[joinMap.Status.JoinNumber]);
            proj.LampHoursFb.LinkInputSig(trilist.UShortInput[joinMap.LampHours.JoinNumber]);
            proj.CurrentInputValueFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelectOffset.JoinNumber]);
            proj.SerialNumberFb.LinkInputSig(trilist.StringInput[joinMap.SerialNumber.JoinNumber]);
        }

        private static void SetupNameFb(EpsonProjector proj, Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, EpsonProjectorJoinMap joinMap)
        {
            var nameFb = new StringFeedback(() => proj.Name);
            nameFb.LinkInputSig(trilist.StringInput[joinMap.Name.JoinNumber]);
            nameFb.FireUpdate();

            var screenNameFb = new StringFeedback(() => proj.ScreenName);
            screenNameFb.LinkInputSig(trilist.StringInput[joinMap.ScreenName.JoinNumber]);
            screenNameFb.FireUpdate();
        }
    }

    public class EpsonProjectorJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("Power On")]
        public JoinDataComplete PowerOn = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 1,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Power On"
            });

        [JoinName("Power Off")]
        public JoinDataComplete PowerOff = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Power Off"
            });

        [JoinName("Warming")]
        public JoinDataComplete Warming = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 3,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Warming"
            });

        [JoinName("Cooling")]
        public JoinDataComplete Cooling = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 4,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Cooling"
            });

        [JoinName("Mute Off")]
        public JoinDataComplete MuteOff = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 8,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Mute Off"
            });

        [JoinName("Mute On")]
        public JoinDataComplete MuteOn = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 9,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Mute On"
            });

        [JoinName("Mute Toggle")]
        public JoinDataComplete MuteToggle = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 10,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Mute Toggle"
            });

        [JoinName("Is Projector")]
        public JoinDataComplete IsProjector = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 21,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Is Projector"
            });

        [JoinName("Is Online")]
        public JoinDataComplete IsOnline = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 50,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Is Online"
            });

        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 1,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial,
                Label = "Name"
            });

        [JoinName("Status")]
        public JoinDataComplete Status = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial,
                Label = "Status"
            });

        [JoinName("Serial Number")]
        public JoinDataComplete SerialNumber = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 3,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial,
                Label = "Serial Number"
            });

        [JoinName("Screen Name")]
        public JoinDataComplete ScreenName = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 4,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial,
                Label = "Screen Name"
            });

        [JoinName("Lamp Hours")]
        public JoinDataComplete LampHours = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog,
                Label = "Lamp Hours"
            });

        [JoinName("Input Select Offset")]
        public JoinDataComplete InputSelectOffset = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 11,
                JoinSpan = 10
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog,
                Label = "Input Select"
            });

        public EpsonProjectorJoinMap(uint joinStart)
            : base(joinStart, typeof(EpsonProjectorJoinMap))
        {

        }
    }
}