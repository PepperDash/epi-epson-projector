using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi.Enums
{
    public class ProjectorInput : CmdEnumeration<ProjectorInput>
    {
        private ProjectorInput(int value, string name, IEpsonCmd cmd, string response)
            : base(value, name, cmd, response)
        {
            
        }

        static ProjectorInput()
        {
            SearchString = "SOURCE";
            Default = Hdmi;
            Unknown = new ProjectorInput(99, "Unknown", CustomCmd.Empty, String.Empty);
        }

        public static readonly ProjectorInput Hdmi = new ProjectorInput(1, "Hdmi", new SourceInputHdmiCmd(), "SOURCE=A0");
        public static readonly ProjectorInput Dvi = new ProjectorInput(2, "Dvi", new SourceInputDviCmd(), "SOURCE=30");
        public static readonly ProjectorInput Computer = new ProjectorInput(3, "Computer", new SourceInputComputerCmd(), "SOURCE=11");
        public static readonly ProjectorInput Video = new ProjectorInput(4, "Video", new SourceInputVideoCmd(), "SOURCE=45");
    }
}