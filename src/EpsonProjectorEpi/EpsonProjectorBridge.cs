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
            trilist.SetUShortSigAction(joinMap.InputSelect.JoinNumber, x => proj.ExecuteSwitchNumeric(x));

            foreach (var input in ProjectorInput.GetAll())
            {
                var inputActual = input;
                var joinActual = inputActual.Value + joinMap.InputSelectOffset.JoinNumber;

                Debug.Console(0, proj, "Mapping {0} to DigitalJoin - {1}", inputActual.Name, joinActual);
                trilist.SetSigTrueAction((uint)joinActual, () => proj.ExecuteSwitch(inputActual));

                var fb = new StringFeedback(() => inputActual.Name);
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
            proj.CurrentInputValueFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelect.JoinNumber]);
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

    public class EpsonProjectorJoinMap : DisplayControllerJoinMap
    {
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

        public EpsonProjectorJoinMap(uint joinStart) : base (joinStart)
        {

        }
    }
}