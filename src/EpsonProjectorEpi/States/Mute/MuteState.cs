﻿using System;
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

        protected MuteState(EpsonProjector proj, IStateManager<ProjectorMute> stateManager)
            : base(proj, stateManager)
        {
            var builder = new StringBuilder(proj.Key);
            builder.Append("-MuteState");

            _key = builder.ToString();    
        }

        public void PowerIsUpdated()
        {
            if (!Proj.Power.PowerIsOn)
                UpdateState(ProjectorMute.MuteOff);
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
            Debug.Console(2, this, "Updating Mute State to: {0}", state.Name);
            Proj.Mute = GetMuteStateForProjector(this, state);
            Dispose();
        }

        public void Poll()
        {
            Proj.EnqueueCmd(new PowerPollCmd());
        }

        protected static MuteState GetMuteStateForProjector(MuteState state, ProjectorMute mute)
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