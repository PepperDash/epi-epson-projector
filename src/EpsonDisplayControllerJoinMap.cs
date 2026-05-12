using System;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace EpsonProjectorEpi
{
    public class EpsonDisplayControllerJoinMap : DisplayControllerJoinMap
    {
        [JoinName("Mute Off")]
        public JoinDataComplete MuteOff = new JoinDataComplete(
            new JoinData { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Description = "Mute Off"
            });

        [JoinName("Mute On")]
        public JoinDataComplete MuteOn = new JoinDataComplete(
            new JoinData { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Description = "Mute On"
            });

        [JoinName("Mute Toggle")]
        public JoinDataComplete MuteToggle = new JoinDataComplete(
            new JoinData { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital,
                Description = "Mute Toggle"
            });

        [JoinName("Mute Off Legacy")]
        public JoinDataComplete MuteOffLegacy = new JoinDataComplete(
            new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Description = "Mute Off Legacy"
            });

        [JoinName("Mute On Legacy")]
        public JoinDataComplete MuteOnLegacy = new JoinDataComplete(
            new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Description = "Mute On Legacy"
            });

        [JoinName("Mute Toggle Legacy")]
        public JoinDataComplete MuteToggleLegacy = new JoinDataComplete(
            new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital,
                Description = "Mute Toggle Legacy"
            });

        [JoinName("Freeze Off")]
        public JoinDataComplete FreezeOff = new JoinDataComplete(
            new JoinData { JoinNumber = 29, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Description = "Freeze Off"
            });

        [JoinName("Freeze On")]
        public JoinDataComplete FreezeOn = new JoinDataComplete(
            new JoinData { JoinNumber = 30, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital,
                Description = "Freeze On"
            });

        [JoinName("Freeze Toggle")]
        public JoinDataComplete FreezeToggle = new JoinDataComplete(
            new JoinData { JoinNumber = 37, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital,
                Description = "Freeze Toggle"
            });

        [JoinName("Lamp Hours")]
        public JoinDataComplete LampHours = new JoinDataComplete(
            new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog,
                Description = "Lamp Hours"
            });

        public EpsonDisplayControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(EpsonDisplayControllerJoinMap))
        {
        }
    }
}
