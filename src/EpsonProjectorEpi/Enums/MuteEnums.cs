using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Enums
{
    public class ProjectorMute : Enumeration<ProjectorMute>
    {
        public IEpsonCmd Command { get; protected set; }

        private ProjectorMute(int value, string name)
            : base (value, name)
        {

        }

        public static readonly ProjectorMute MuteOn = new ProjectorMute(1, "Mute On") { Command = new MuteOnCmd() };
        public static readonly ProjectorMute MuteOff = new ProjectorMute(0, "Mute Off") { Command = new MuteOffCmd() };
    }
}