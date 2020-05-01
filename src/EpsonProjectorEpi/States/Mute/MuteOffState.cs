using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi.States.Mute
{
    public class MuteOffState : MuteState
    {
        public MuteOffState(MuteState state)
            : this(state.Proj)
        {

        }

        public MuteOffState(EpsonProjector proj)
            : base(proj)
        {
            _currentState = ProjectorMute.MuteOff;
        }

        public override void MuteOn()
        {
            SendMuteCmd(ProjectorMute.MuteOn);
        }

        public override void MuteOff()
        {
            Poll();
        }

        public override void MuteToggle()
        {
            MuteOn();
        }

        public override bool MuteIsOn
        {
            get { return false; }
        }
    }
}