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
    public class PowerCoolingState : PowerState
    {
        public PowerCoolingState(PowerState state)
            : this(state.Proj, state.StateManager)
        {

        }

        public PowerCoolingState(EpsonProjector proj, IStateManager<ProjectorPower> stateManager)
            : base(proj, stateManager)
        {
            _currentState = ProjectorPower.Cooling;
        }

        public override void PowerOn()
        {
            Poll();
        }

        public override void PowerOff()
        {
            return;
        }

        public override void PowerToggle()
        {
            Poll();
        }

        public override bool PowerIsOn
        {
            get { return false; }
        }

        public override bool ProjectorIsWarming
        {
            get { return false; ; }
        }

        public override bool ProjectorIsCooling
        {
            get { return true; }
        }
    }
}