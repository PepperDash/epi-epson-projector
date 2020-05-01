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
    public class PowerOnState : PowerState
    {
        public PowerOnState(PowerState state)
            : this(state.Proj)
        {

        }

        public PowerOnState(EpsonProjector proj)
            : base(proj)
        {
            _currentState = ProjectorPower.PowerOn;
        }

        public override void PowerOn()
        {
            return;
        }

        public override void PowerOff()
        {
            SendPwrCmd(ProjectorPower.PowerOff);
        }

        public override void PowerToggle()
        {
            PowerOff();
        }

        public override bool PowerIsOn
        {
            get { return true; }
        }

        public override bool ProjectorIsWarming
        {
            get { return false; ; }
        }

        public override bool ProjectorIsCooling
        {
            get { return false; }
        }
    }
}