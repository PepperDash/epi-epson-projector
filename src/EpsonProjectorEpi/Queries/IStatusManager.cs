using System;

namespace EpsonProjectorEpi.Queries
{
    public interface IStatusManager
    {
        EpsonProjectorEpi.Enums.ProjectorInput InputStatus { get; set; }
        EpsonProjectorEpi.Enums.ProjectorPower PowerStatus { get; set; }
        EpsonProjectorEpi.Enums.ProjectorMute MuteStatus { get; set; }
        int LampHours { get; }
        string SerialNumber { get; }
        event EventHandler StatusUpdated;
    }
}
