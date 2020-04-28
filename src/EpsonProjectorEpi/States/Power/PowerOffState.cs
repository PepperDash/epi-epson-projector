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
    public class PowerOffState : PowerState
    {
        public PowerOffState(PowerState state)
            : this(state.Proj, state.StateManager)
        {

        }

        public PowerOffState(EpsonProjector proj, IStateManager<ProjectorPower> stateManager)
            : base(proj, stateManager)
        {
            _currentState = ProjectorPower.PowerOff;
        }

        public override void PowerOn()
        {
            SendPwrCmd(ProjectorPower.PowerOn);
        }

        public override void PowerOff()
        {
            Poll();
        }

        public override void PowerToggle()
        {
            PowerOn();
        }

        public override bool PowerIsOn
        {
            get { return false; }
        }

        public override bool ProjectorIsWarming
        {
            get { return false; }
        }

        public override bool ProjectorIsCooling
        {
            get { return false; }
        }
    }
}