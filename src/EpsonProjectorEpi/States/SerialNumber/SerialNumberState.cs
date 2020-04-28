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

namespace EpsonProjectorEpi.States.SerialNumber
{
    public class SerialNumberState : BaseState<string>
    {
        private readonly string _key;

        protected SerialNumberState(SerialNumberState state, string serialNumber)
            : this(state.Proj, state.StateManager)
        {
            _currentState = serialNumber;
        }

        public SerialNumberState(EpsonProjector proj, IStateManager<string> stateManager)
            : base(proj, stateManager)
        {
            var builder = new StringBuilder(proj.Key);
            builder.Append("-SerialNumber");

            _key = builder.ToString();
        }

        protected override void UpdateState(string state)
        {
            if (state == Current)
                return;

            Proj.SerialNumber = new SerialNumberState(this, state);
            Dispose();
        }

        public override string Key
        {
            get { return _key; }
        }
    }
}