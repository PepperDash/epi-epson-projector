using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Enums
{
    public class ProjectorPower : CmdEnumeration<ProjectorPower>
    {
        private ProjectorPower(int value, string name, IEpsonCmd cmd, string response)
            : base (value, name, cmd, response)
        {
            
        }

        static ProjectorPower()
        {
            SearchString = "PWR";
            Default = ProjectorPower.PowerOff;
            Unknown = new ProjectorPower(99, "Unknown", CustomCmd.Empty, String.Empty);
        }

        public static readonly ProjectorPower PowerOff = new ProjectorPower(0, "Power Off", new PowerOffCmd(), "PWR=00");
        public static readonly ProjectorPower PowerOn = new ProjectorPower(1, "Power On", new PowerOnCmd(), "PWR=01");
        public static readonly ProjectorPower Warming = new ProjectorPower(2, "Warming", CustomCmd.Empty, "PWR=02");
        public static readonly ProjectorPower Cooling = new ProjectorPower(3, "Cooling", CustomCmd.Empty, "PWR=03");
        public static readonly ProjectorPower Standby = new ProjectorPower(4, "Standby", CustomCmd.Empty, "PWR=04");
        public static readonly ProjectorPower AbnormalityStandby = new ProjectorPower(5, "Abnormality Standby", CustomCmd.Empty, "PWR=05");
    }
}