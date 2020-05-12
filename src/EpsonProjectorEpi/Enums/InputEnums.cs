using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi.Enums
{
    public abstract class ProjectorInput : CmdEnumeration<ProjectorInput>
    {
        private ProjectorInput(int value, string name)
            : base(value, name)
        {
            SearchString = "SOURCE";
        }

        public static readonly ProjectorInput Hdmi = new InputHdmiEnum();
        public static readonly ProjectorInput Dvi = new InputDviEnum();
        public static readonly ProjectorInput Computer = new InputComputerEnum();
        public static readonly ProjectorInput Video = new InputVideoEnum();

        private sealed class InputHdmiEnum : ProjectorInput
        {
            public InputHdmiEnum()
                : base (1, "Hdmi")
            {

            }

            public override IEpsonCmd Cmd { get { return new SourceInputHdmiCmd(); } }
            public override string  Response { get { return "SOURCE=A0"; } }
        }

        private sealed class InputDviEnum : ProjectorInput
        {
            public InputDviEnum()
                : base(2, "DVI")
            {

            }

            public override IEpsonCmd Cmd { get { return new SourceInputDviCmd(); } }
            public override string Response { get { return "SOURCE=30"; } }
        }

        private class InputComputerEnum : ProjectorInput
        {
            public InputComputerEnum()
                : base(3, "Computer")
            {

            }

            public override IEpsonCmd Cmd { get { return new SourceInputComputerCmd(); } }
            public override string Response { get { return "SOURCE=11"; } }
        }

        private sealed class InputVideoEnum : ProjectorInput
        {
            public InputVideoEnum()
                : base(4, "Video")
            {

            }

            public override IEpsonCmd Cmd { get { return new SourceInputVideoCmd(); } }
            public override string Response { get { return "SOURCE=45"; } }
        }
    }
}