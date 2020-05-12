using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Enums
{
    public abstract class ProjectorPower : CmdEnumeration<ProjectorPower>
    {
        private ProjectorPower(int value, string name)
            : base (value, name)
        {
            SearchString = "PWR";
        }

        public abstract ProjectorPower Next { get; }

        public static readonly ProjectorPower PowerOff = new PowerOffEnum();
        public static readonly ProjectorPower PowerOn = new PowerOnEnum();
        public static readonly ProjectorPower Warming = new PowerWarmingEnum();
        public static readonly ProjectorPower Cooling = new PowerCoolingEnum();
        public static readonly ProjectorPower Standby = new PowerStandbyEnum();
        public static readonly ProjectorPower AbnormalityStandby = new PowerAbnormalStandbyEnum();
        public static readonly ProjectorPower Unknown = new UnknownEnum();

        private sealed class PowerOffEnum : ProjectorPower
        {
            public PowerOffEnum()
                : base(0, "Power Off")
            {

            }

            public override ProjectorPower Next { get { return ProjectorPower.Warming; } }
            public override IEpsonCmd Cmd { get { return new PowerOffCmd(); } }
            public override string Response { get { return "PWR=00"; } }
        }

        private sealed class PowerOnEnum : ProjectorPower
        {
            public PowerOnEnum()
                : base(1, "Power On")
            {

            }

            public override ProjectorPower Next { get { return ProjectorPower.Cooling; } }
            public override IEpsonCmd Cmd { get { return new PowerOnCmd(); } }
            public override string Response { get { return "PWR=01"; } }
        }

        private sealed class PowerWarmingEnum : ProjectorPower
        {
            public PowerWarmingEnum()
                : base(2, "Warming")
            {

            }

            public override ProjectorPower Next { get { return ProjectorPower.PowerOn; } }
            public override IEpsonCmd Cmd { get { return CustomCmd.Empty; } }
            public override string Response { get { return "PWR=02"; } }
        }

        private sealed class PowerCoolingEnum : ProjectorPower
        {
            public PowerCoolingEnum()
                : base(3, "Cooling")
            {

            }

            public override ProjectorPower Next { get { return ProjectorPower.PowerOff; } }
            public override IEpsonCmd Cmd { get { return CustomCmd.Empty; } }
            public override string Response { get { return "PWR=03"; } }
        }

        private sealed class PowerStandbyEnum : ProjectorPower
        {
            public PowerStandbyEnum()
                : base(4, "Standby")
            {

            }

            public override ProjectorPower Next { get { return ProjectorPower.Warming; } }
            public override IEpsonCmd Cmd { get { return CustomCmd.Empty; } }
            public override string Response { get { return "PWR=04"; } }
        }

        private sealed class PowerAbnormalStandbyEnum : ProjectorPower
        {
            public PowerAbnormalStandbyEnum()
                : base(5, "Abnormal Standby")
            {

            }

            public override ProjectorPower Next { get { return ProjectorPower.Warming; } }
            public override IEpsonCmd Cmd { get { return CustomCmd.Empty; } }
            public override string Response { get { return "PWR=05"; } }
        }

        private sealed class UnknownEnum : ProjectorPower
        {
            public UnknownEnum()
                : base(99, "Unknown")
            {

            }

            public override ProjectorPower Next { get { return ProjectorPower.Unknown as ProjectorPower; } }
            public override IEpsonCmd Cmd { get { return CustomCmd.Empty; } }
            public override string Response { get { return String.Empty; } }
        }
    }
}