using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Bridges;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi
{
    public static class EpsonProjectorBridge
    {
        public static void LinkToApiExt(this EpsonProjector proj, Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = new EpsonProjectorJoinMap(proj.Key, joinStart);

            SetupNameFb(proj, trilist, joinMap);

            var isProjFb = new BoolFeedback(() => true);
            isProjFb.LinkInputSig(trilist.BooleanInput[joinMap.IsProjector]);
            isProjFb.FireUpdate();

            trilist.SetSigTrueAction(joinMap.PowerOn, proj.PowerOn);
            trilist.SetSigTrueAction(joinMap.PowerOff, proj.PowerOff);
            trilist.SetSigTrueAction(joinMap.MuteOn, proj.MuteOn);
            trilist.SetSigTrueAction(joinMap.MuteOff, proj.MuteOff);
            trilist.SetSigTrueAction(joinMap.MuteToggle, proj.MuteToggle);
            trilist.SetUShortSigAction(joinMap.InputSelect, x => proj.Input.SetInput(x));

            foreach (var input in ProjectorInput.GetAll())
            {
                if (input == ProjectorInput.Unknown) 
                    continue;

                var inputActual = input;
                var joinActual = inputActual.Value + joinMap.InputSelectOffset;

                Debug.Console(0, proj, "Mapping {0} to DigitalJoin - {1}", inputActual.Name, joinActual);
                trilist.SetSigTrueAction((uint)joinActual, () => proj.Input.SetInput(inputActual));

                var fb = new StringFeedback(() => inputActual.Name);
                fb.LinkInputSig(trilist.StringInput[(uint)joinActual]);
                fb.FireUpdate();
            }

            proj.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn]);
            proj.PowerIsOffFb.LinkInputSig(trilist.BooleanInput[joinMap.PowerOff]);
            proj.IsWarmingUpFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Warming]);
            proj.IsCoolingDownFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Cooling]);
            proj.MuteIsOnFb.LinkInputSig(trilist.BooleanInput[joinMap.MuteOn]);
            proj.MuteIsOffFb.LinkInputSig(trilist.BooleanInput[joinMap.MuteOff]);
            proj.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            proj.StatusFb.LinkInputSig(trilist.StringInput[joinMap.Status]);
            proj.LampHoursFb.LinkInputSig(trilist.UShortInput[joinMap.LampHours]);
            proj.CurrentInputValueFb.LinkInputSig(trilist.UShortInput[joinMap.InputSelect]);
            proj.SerialNumberFb.LinkInputSig(trilist.StringInput[joinMap.SerialNumber]);
        }

        private static void SetupNameFb(EpsonProjector proj, Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, EpsonProjectorJoinMap joinMap)
        {
            var nameFb = new StringFeedback(() => proj.Name);
            nameFb.LinkInputSig(trilist.StringInput[joinMap.Name]);
            nameFb.FireUpdate();

            var screenNameFb = new StringFeedback(() => proj.ScreenName);
            screenNameFb.LinkInputSig(trilist.StringInput[joinMap.ScreenName]);
            screenNameFb.FireUpdate();
        }
    }

    public class EpsonProjectorJoinMap : DisplayControllerJoinMap, IKeyed
    {
        public uint Warming { get; set; }
        public uint Cooling { get; set; }
        public uint MuteOn { get; set; }
        public uint MuteOff { get; set; }
        public uint MuteToggle { get; set; }
        public uint IsProjector { get; set; }
        public uint Status { get; set; }
        public uint SerialNumber { get; set; }
        public uint ScreenName { get; set; }
        public uint LampHours { get; set; }

        private readonly string _key;

        EpsonProjectorJoinMap() : base() 
        {
            Warming = 3;
            Cooling = 4;

            MuteOff = 8;
            MuteOn = 9;
            MuteToggle = 10;
            IsProjector = 21;

            Status = 2;
            SerialNumber = 3;
            ScreenName = 4;

            LampHours = 2;
        }

        public EpsonProjectorJoinMap(string key, uint joinStart) : this()
        {
            _key = key;
            OffsetJoinNumbers(joinStart);
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {   
            var joinActual = joinStart - 1;
            var props = GetType().GetCType().GetProperties().Where(t => t.PropertyType == typeof(uint));

            foreach (var prop in props)
            {
                var value = (uint)prop.GetValue(this, null);
                prop.SetValue(this, (value + joinActual), null);

                Debug.Console(0, this, "Setting Join:{0} to value:{1}", prop.Name, prop.GetValue(this, null));
            }
        }

        #region IKeyed Members

        public string Key
        {
            get { return _key; }
        }

        #endregion
    }
}