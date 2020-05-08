using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Queries;
using EpsonProjectorEpi.States;

namespace EpsonProjectorEpi.States.Mute
{
    public abstract class MuteState : BaseState<ProjectorMute>, IStateHasPoll
    {
        private readonly string _key;

        public abstract void MuteOn();
        public abstract void MuteOff();
        public abstract void MuteToggle();

        public abstract bool MuteIsOn { get; }

        protected bool _confirmed;
        public bool Confirmed
        {
            get { return _confirmed; }
            set
            {
                if (_confirmed)
                    return;

                Debug.Console(0, this, "Mute state confirmed : {0}", _currentState.Name);
                _confirmed = value;
            }
        }

        protected MuteState(EpsonProjector proj)
            : base(proj)
        {
            var builder = new StringBuilder(proj.Key);
            builder.Append("-MuteState");

            _key = builder.ToString();    
        }

        protected void SendMuteCmd(ProjectorMute mute)
        {
            if (mute == _currentState || !Proj.Power.PowerIsOn)
                return;

            Proj.EnqueueCmd(mute.Cmd);
            UpdateState(mute);
        }

        protected override void UpdateState(ProjectorMute state)
        {
            if (state == Current)
                return;

            Debug.Console(0, this, "Updating Mute State to: {0}", state.Name);
            Proj.Mute = GetMuteStateForProjector(this, state);
        }

        public void Poll()
        {
            Proj.EnqueueCmd(new PowerPollCmd());
        }

        public static MuteState GetMuteStateForProjector(MuteState state, ProjectorMute mute)
        {
            if (mute == ProjectorMute.MuteOn)
            {
                return new MuteOnState(state);
            }

            if (mute == ProjectorMute.MuteOff)
            {
                return new MuteOffState(state);
            }

            throw new ArgumentException(mute.Name);
        }

        public override string Key
        {
            get { return _key; }
        }
    }
}