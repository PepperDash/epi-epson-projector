using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Enums
{
    public abstract class ProjectorPower : Enumeration<ProjectorPower>
    {
        public IEpsonCmd Command { get; protected set; }

        private ProjectorPower(int value, string name)
            : base (value, name)
        {
            
        }

        public static readonly ProjectorPower PowerOff = new PowerOffEnum() { Command = new PowerOffCmd() };
        public static readonly ProjectorPower PowerOn = new PowerOnEnum() { Command = new PowerOnCmd() };
        public static readonly ProjectorPower Warming = new PowerWarmingEnum() { Command = CustomCmd.Empty };
        public static readonly ProjectorPower Cooling = new PowerCoolingEnum() { Command = CustomCmd.Empty };
        
        private sealed class PowerOffEnum : ProjectorPower
        {
            public PowerOffEnum()
                : base(0, "Power Off")
            {
                Command = new PowerOffCmd();
            }
        }

        private sealed class PowerOnEnum : ProjectorPower
        {
            public PowerOnEnum()
                : base(1, "Power On")
            {
                Command = new PowerOnCmd();
            }
        }

        private sealed class PowerWarmingEnum : ProjectorPower
        {
            public PowerWarmingEnum()
                : base(2, "Warming")
            {
                Command = CustomCmd.Empty;
            }
        }

        private sealed class PowerCoolingEnum : ProjectorPower
        {
            public PowerCoolingEnum()
                : base(3, "Cooling")
            {
                Command = CustomCmd.Empty;
            }

        }
    }
}