using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;


namespace EpsonProjectorEpi.States.Mute
{
    public class MuteOnState : MuteState
    {
        public MuteOnState(MuteState state)
            : this(state.Proj)
        {

        }

        public MuteOnState(EpsonProjector proj)
            : base(proj)
        {
            _currentState = ProjectorMute.MuteOn;
        }

        public override void MuteOn()
        {
            Poll();
        }

        public override void MuteOff()
        {
            SendMuteCmd(ProjectorMute.MuteOff);
        }

        public override void MuteToggle()
        {
            MuteOff();
        }

        public override bool MuteIsOn
        {
            get { return true; }
        }
    }
}