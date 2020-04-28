using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Enums
{
    public class ProjectorMute : CmdEnumeration<ProjectorMute>
    {
        private ProjectorMute(int value, string name, IEpsonCmd cmd, string response)
            : base (value, name, cmd, response)
        {
            
        }

        static ProjectorMute()
        {
            SearchString = "MUTE";
            Default = MuteOff;
            Unknown = new ProjectorMute(99, "Mute Unknown", CustomCmd.Empty, String.Empty);
        }

        public static readonly ProjectorMute MuteOn = new ProjectorMute(1, "Mute On", new MuteOnCmd(), "MUTE=ON");
        public static readonly ProjectorMute MuteOff = new ProjectorMute(0, "Mute Off", new MuteOffCmd(), "MUTE=OFF");
    }
}