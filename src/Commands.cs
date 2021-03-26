using System;
using PepperDash.Core;
using PepperDash.Essentials.Core.Queues;

namespace EpsonProjectorEpi
{
    public static class Commands
    {
        public class EpsonCommand : IQueueMessage
        {
            public IBasicCommunication Coms { get; set; }
            public string Message { get; set; }

            public void Dispatch()
            {
                if (Coms == null || String.IsNullOrEmpty(Message))
                    return;

                Coms.SendText(Message + "\x0D");
            }
        }

        public const string SourceComputer = "SOURCE 11";
        public const string SourceHdmi = "SOURCE 30";
        public const string SourceVideo = "SOURCE 45";
        public const string SourceDvi = "SOURCE A0";
        public const string MuteOn = "MUTE ON";
        public const string MuteOff = "MUTE OFF";
        public const string PowerOn = "PWR ON";
        public const string PowerOff = "PWR OFF";
        public const string PowerPoll = "PWR?";
        public const string SourcePoll = "SOURCE?";
        public const string LampPoll = "LAMP?";
        public const string MutePoll = "MUTE?";
        public const string SerialNumberPoll = "SNO?";
    }
}