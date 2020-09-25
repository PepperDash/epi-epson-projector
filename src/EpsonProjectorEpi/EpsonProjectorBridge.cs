using System;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
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

            for (var x = 0; x < joinMap.InputSelectOffset.JoinNumber; x++)
                LinkInputSelect(proj, trilist, joinMap, x);

            Debug.Console(1, proj, "Linking Power On Feedback...");
            proj.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn.JoinNumber]);

            Debug.Console(1, proj, "Linking Warming Feedback...");
            proj.IsWarmingUpFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Warming.JoinNumber]);

            Debug.Console(1, proj, "Linking Cooling Feedback...");
            proj.IsCoolingDownFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Cooling.JoinNumber]);

            Debug.Console(1, proj, "Linking Mute On Feedback...");
            proj.MuteIsOnFb.LinkInputSig(trilist.BooleanInput[joinMap.MuteOn.JoinNumber]);

            Debug.Console(1, proj, "Linking Mute Off Feedback...");
            proj.MuteIsOffFb.LinkInputSig(trilist.BooleanInput[joinMap.MuteOff.JoinNumber]);

            Debug.Console(1, proj, "Linking Communication Monitor Feedback...");
            proj.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

            Debug.Console(1, proj, "Linking Status Feedback...");
            proj.StatusFb.LinkInputSig(trilist.StringInput[joinMap.Status.JoinNumber]);

            Debug.Console(1, proj, "Linking Lampt Hours Feedback...");
            proj.LampHoursFb.LinkInputSig(trilist.UShortInput[joinMap.LampHours.JoinNumber]);

            Debug.Console(1, proj, "Linking Current Input Feedback...");
            proj.CurrentInputValueFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelectOffset.JoinNumber]);

            Debug.Console(1, proj, "Linking Serial Number Feedback...");
            proj.SerialNumberFb.LinkInputSig(trilist.StringInput[joinMap.SerialNumber.JoinNumber]);
        }

        private static void LinkInputSelect(EpsonProjector proj, BasicTriList trilist, EpsonProjectorJoinMap joinMap, int x)
        {
            var joinActual = joinMap.InputSelectOffset.JoinNumber + x;
            var inputActual = x + 1;

            var routingPort =
                proj.InputPorts.Where(port => port.Port != null)
                    .FirstOrDefault(t => Convert.ToInt32(t.Port) == inputActual);

            if (routingPort == null) return;

            Debug.Console(1, proj, "Linking input:{0} to join {1}", routingPort.Key, joinActual);

            trilist.SetSigTrueAction((uint) joinActual, () => proj.ExecuteSwitch(routingPort.Selector));

            var fb = new StringFeedback(() => routingPort.Key);
            fb.LinkInputSig(trilist.StringInput[(uint) joinActual]);
            fb.FireUpdate();
        }

        private static void SetupNameFb(EpsonProjector proj, BasicTriList trilist, EpsonProjectorJoinMap joinMap)
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
        [JoinName("Power Off")]
        public JoinDataComplete PowerOff = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 1,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Power Off"
            });

        [JoinName("Power On")]
        public JoinDataComplete PowerOn = new JoinDataComplete(
            new JoinData()
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata()
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Label = "Power On"
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
                JoinNumber = 5,
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
                JoinNumber = 6,
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
                JoinNumber = 7,
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
                JoinNumber = 8,
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