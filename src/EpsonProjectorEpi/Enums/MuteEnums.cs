using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Enums
{
    public abstract class ProjectorMute : CmdEnumeration<ProjectorMute>
    {
        private ProjectorMute(int value, string name)
            : base (value, name)
        {
            
        }

        static ProjectorMute()
        {
            SearchString = "MUTE";
            Default = MuteOff;
            Unknown = new UnknownEnum();
        }

        public static readonly ProjectorMute MuteOn = new MuteOnEnum();
        public static readonly ProjectorMute MuteOff = new MuteOffEnum();

        private class MuteOnEnum : ProjectorMute
        {
            public MuteOnEnum()
                : base(1, "Mute On")
            {

            }

            public override IEpsonCmd Cmd
            {
                get { return new MuteOnCmd(); }
            }

            public override string Response
            {
                get { return "MUTE=ON"; }
            }
        }

        private class MuteOffEnum : ProjectorMute
        {
            public MuteOffEnum()
                : base(0, "Mute Off")
            {

            }

            public override IEpsonCmd Cmd
            {
                get { return new MuteOffCmd(); }
            }

            public override string Response
            {
                get { return "MUTE=OFF"; }
            }
        }

        private class UnknownEnum : ProjectorMute
        {
            public UnknownEnum()
                : base(99, "Unknown")
            {

            }

            public override IEpsonCmd Cmd
            {
                get { return CustomCmd.Empty; }
            }

            public override string Response
            {
                get { return String.Empty; }
            }
        }
    }
}