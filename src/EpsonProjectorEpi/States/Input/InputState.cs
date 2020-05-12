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

namespace EpsonProjectorEpi.States.Input
{
    public abstract class InputState : BaseState<ProjectorInput>, IStateHasPoll
    {
        private readonly string _key;

        protected bool _confirmed;
        public bool Confirmed
        {
            get { return _confirmed; }
            set
            {
                if (_confirmed)
                    return;

                Debug.Console(0, this, "Input state confirmed : {0}", _currentState.Name);
                _confirmed = value;
            }
        }

        public int InputNumber 
        { 
            get 
            {
                return Proj.Power.PowerIsOn ? _currentState.Value : 0;
            } 
        }

        public string InputName
        {
            get
            {
                return Proj.Power.PowerIsOn ? _currentState.Name : "Inactive"; 
            }
        }

        public virtual void SetInput(int input)
        {
            if (input == 0)
            {
                Proj.MuteOn();
                return;
            }

            ProjectorInput newInput;
            if (!ProjectorInput.TryFromValue(input, out newInput))
                return;

            SetInput(newInput);
        }

        public void SetInput(ProjectorInput input)
        {
            Proj.MuteOff();
            Proj.PowerOn();
            UpdateState(input);
        }

        protected InputState(EpsonProjector proj)
            : base(proj)
        {
            var builder = new StringBuilder(proj.Key);
            builder.Append("-InputState");

            _key = builder.ToString();
        }

        public void Poll()
        {
            Proj.EnqueueCmd(new SourcePollCmd());
        }

        protected override void UpdateState(ProjectorInput state)
        {
            if (_currentState == state)
                return;

            Debug.Console(0, this, "Updating projector input to: {0}", state.Name);
            Proj.EnqueueCmd(state.Cmd);
            Proj.Input = GetStateForProjectorInput(this, state);
        }

        public static InputState GetStateForProjectorInput(InputState state, ProjectorInput input)
        {
            if (input == ProjectorInput.Hdmi)
                return new InputStateHdmi(state);

            if (input == ProjectorInput.Computer)
                return new InputStateComputer(state);

            if (input == ProjectorInput.Dvi)
                return new InputStateDvi(state);

            if (input == ProjectorInput.Video)
                return new InputStateVideo(state);

            throw new ArgumentException(input.Name);
        }

        public override string Key
        {
            get { return _key; }
        }
    }
}