using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.States.Power
{
    public class PowerWarmingState : PowerState
    {
        public PowerWarmingState(PowerState state)
            : this(state.Proj)
        {

        }

        public PowerWarmingState(EpsonProjector proj)
            : base(proj)
        {
            _currentState = ProjectorPower.Warming;
        }

        public override void PowerOn()
        {
            return;
        }

        public override void PowerOff()
        {
            return;
        }

        public override void PowerToggle()
        {
            return;
        }

        public override bool PowerIsOn
        {
            get { return false; }
        }

        public override bool ProjectorIsWarming
        {
            get { return true; }
        }

        public override bool ProjectorIsCooling
        {
            get { return false; }
        }

        static void TimerCallback(object obj)
        {

        }
    }
}