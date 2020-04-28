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

namespace EpsonProjectorEpi.States.LampHours
{
    public class LampHoursState : BaseState<int>
    {
        private readonly string _key;

        protected LampHoursState(LampHoursState state, int lampHours)
            : this(state.Proj, state.StateManager)
        {
            _currentState = lampHours;
        }

        public LampHoursState(EpsonProjector proj, IStateManager<int> stateManager)
            : base(proj, stateManager)
        {
            var builder = new StringBuilder(proj.Key);
            builder.Append("-LampHours");

            _key = builder.ToString();
        }

        protected override void UpdateState(int state)
        {
            if (state == Current)
                return;

            Proj.LampHours = new LampHoursState(this, state);
            Dispose();
        }

        public override string Key
        {
            get { return _key; }
        }
    }
}