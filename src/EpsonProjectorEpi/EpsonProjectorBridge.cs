using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Bridges;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi
{
    public static class EpsonProjectorBridge
    {
        public static void LinkToApiExt(this EpsonProjector proj, Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = new EpsonProjectorJoinMap(joinStart);

            var nameFb = new StringFeedback(() => proj.Name);
            nameFb.LinkInputSig(trilist.StringInput[joinMap.Name]);
            nameFb.FireUpdate();

            var screenNameFb = new StringFeedback(() => proj.ScreenName);
            screenNameFb.LinkInputSig(trilist.StringInput[joinMap.ScreenName]);
            screenNameFb.FireUpdate();

            var isProjFb = new BoolFeedback(() => true);
            isProjFb.LinkInputSig(trilist.BooleanInput[joinMap.IsProjector]);
            isProjFb.FireUpdate();

            trilist.SetSigTrueAction(joinMap.PowerOn, proj.PowerOn);
            trilist.SetSigTrueAction(joinMap.PowerOff, proj.PowerOff);
            trilist.SetSigTrueAction(joinMap.MuteOn, proj.MuteOn);
            trilist.SetSigTrueAction(joinMap.MuteOff, proj.MuteOff);
            trilist.SetSigTrueAction(joinMap.MuteToggle, proj.MuteToggle);
            trilist.SetUShortSigAction(joinMap.InputSelect, x => proj.ExecuteSwitch(x));

            foreach (var input in ProjectorInput.GetAll())
            {
                var joinActual = input.Value + joinMap.InputSelectOffset;
                trilist.SetSigTrueAction((uint)joinActual, () => proj.ExecuteSwitch(input));

                var fb = new StringFeedback(() => input.Name);
                fb.LinkInputSig(trilist.StringInput[(uint)joinActual]);
                fb.FireUpdate();
            }

            proj.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn]);
            proj.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff]);
            proj.IsWarmingUpFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Warming]);
            proj.IsCoolingDownFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.Cooling]);
            proj.MuteIsOnFb.LinkInputSig(trilist.BooleanInput[joinMap.MuteOn]);
            proj.MuteIsOnFb.LinkComplementInputSig(trilist.BooleanInput[joinMap.MuteOff]);
            proj.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            proj.StatusFb.LinkInputSig(trilist.StringInput[joinMap.Status]);
            proj.LampHoursFb.LinkInputSig(trilist.UShortInput[joinMap.LampHours]);
            proj.CurrentInputValueFb.LinkInputSig(trilist.UShortInput[joinMap.InputSelect]);
            proj.SerialNumberFb.LinkInputSig(trilist.StringInput[joinMap.SerialNumber]);
        }
    }

    public class EpsonProjectorJoinMap : DisplayControllerJoinMap
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

        EpsonProjectorJoinMap() : base() 
        {
            Warming = 3;
            Cooling = 4;

            MuteOff = 5;
            MuteOn = 6;
            MuteToggle = 7;
            IsProjector = 8;

            Status = 2;
            SerialNumber = 3;
            ScreenName = 4;

            LampHours = 2;
        }

        public EpsonProjectorJoinMap(uint joinStart) : this()
        {
            OffsetJoinNumbers(joinStart);
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {   
            base.OffsetJoinNumbers(joinStart);

            Warming = Warming.Offset(joinStart);
            Cooling = Cooling.Offset(joinStart);
            MuteOff = MuteOff.Offset(joinStart);
            MuteOn = MuteOn.Offset(joinStart);
            MuteToggle = MuteToggle.Offset(joinStart);
            IsProjector = IsProjector.Offset(joinStart);
            Status = Status.Offset(joinStart);
            SerialNumber = SerialNumber.Offset(joinStart);
            ScreenName = ScreenName.Offset(joinStart);
            LampHours = LampHours.Offset(joinStart);
        }      
    }

    static class JoinMapExtensions
    {
        public static uint Offset(this uint join, uint joinStart)
        {
            var joinActual = joinStart - 1;
            join = join + joinActual;

            return join;
        }
    }
}