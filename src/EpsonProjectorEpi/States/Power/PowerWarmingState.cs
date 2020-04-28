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
            : this(state.Proj, state.StateManager)
        {

        }

        public PowerWarmingState(EpsonProjector proj, IStateManager<ProjectorPower> stateManager)
            : base(proj, stateManager)
        {
            _currentState = ProjectorPower.Warming;
        }

        public override void PowerOn()
        {
            Poll();
        }

        public override void PowerOff()
        {
            Poll();
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
    }
}