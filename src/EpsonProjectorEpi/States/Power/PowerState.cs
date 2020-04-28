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

namespace EpsonProjectorEpi.States.Power
{
    public abstract class PowerState : BaseState<ProjectorPower>, IStateHasPoll
    {
        private readonly string _key;

        public abstract void PowerOn();
        public abstract void PowerOff();
        public abstract void PowerToggle();

        public abstract bool PowerIsOn { get; }
        public abstract bool ProjectorIsWarming { get; }
        public abstract bool ProjectorIsCooling { get; }

        protected PowerState(EpsonProjector proj, IStateManager<ProjectorPower> stateManager)
            : base(proj, stateManager)
        {
            var builder = new StringBuilder(proj.Key);
            builder.Append("-PowerState");

            _key = builder.ToString();
        }

        protected void SendPwrCmd(ProjectorPower power)
        {
            if (power == _currentState || power == ProjectorPower.Unknown)
                return;

            Proj.EnqueueCmd(power.Cmd);
            UpdateState(power);
        }

        protected override void UpdateState(ProjectorPower state)
        {
            Debug.Console(2, this, "Updating projector power to: {0}", state.Name);
            Proj.Power = GetPowerStateForProjectorPower(this, state);
            Dispose();
        }

        protected static PowerState GetPowerStateForProjectorPower(PowerState state, ProjectorPower power)
        {
            if (power == ProjectorPower.PowerOn)
                return new PowerOnState(state);

            if (power == ProjectorPower.Warming)
                return new PowerWarmingState(state);

            if (power == ProjectorPower.Standby)
                return new PowerOffState(state);

            if (power == ProjectorPower.PowerOff)
                return new PowerOffState(state);

            if (power == ProjectorPower.Cooling)
                return new PowerCoolingState(state);

            if (power == ProjectorPower.AbnormalityStandby)
            {
                Debug.ConsoleWithLog(0, state, "Projector is in Abnormal Standby mode");
                return new PowerOffState(state);
            }

            throw new ArgumentException(power.Name);
        }

        public override string Key
        {
            get { return _key; }
        }

        #region IStateHasPoll Members

        public void Poll()
        {
            Proj.EnqueueCmd(new PowerPollCmd());
        }

        #endregion
    }
}