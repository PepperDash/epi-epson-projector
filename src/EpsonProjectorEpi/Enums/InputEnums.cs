using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Enums
{
    public abstract class ProjectorInput : Enumeration<ProjectorInput>
    {
        public IEpsonCmd Command { get; protected set; }

        private ProjectorInput(int value, string name)
            : base(value, name)
        {
 
        }

        public static readonly ProjectorInput Hdmi = new InputHdmiEnum();
        public static readonly ProjectorInput Dvi = new InputDviEnum();
        public static readonly ProjectorInput Computer = new InputComputerEnum();
        public static readonly ProjectorInput Video = new InputVideoEnum();

        private sealed class InputDviEnum : ProjectorInput
        {
            public InputDviEnum()
                : base(1, "DVI")
            {
                Command = new SourceInputDviCmd();
            }
        }

        private sealed class InputHdmiEnum : ProjectorInput
        {
            public InputHdmiEnum()
                : base(2, "Hdmi")
            {
                Command = new SourceInputHdmiCmd();
            }
        }

        private class InputComputerEnum : ProjectorInput
        {
            public InputComputerEnum()
                : base(3, "Computer")
            {
                Command = new SourceInputComputerCmd();
            }
        }

        private sealed class InputVideoEnum : ProjectorInput
        {
            public InputVideoEnum()
                : base(4, "Video")
            {
                Command = new SourceInputVideoCmd();
            }
        }
    }
}