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

        private ProjectorInput _requestedInput = null;
        private bool _requestedPower = false;

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

        public void SetInput(int input)
        {
            if (CheckForProjectorMute(input))
                return;

            ProjectorInput newInput;
            if (!ProjectorInput.TryFromValue(input, out newInput))
                return;

            if (newInput == _currentState)
                return;  

            if (Proj.Power.PowerIsOn)
            {
                SendInputCmd(newInput);
            }
            else if (Proj.Power.ProjectorIsWarming)
            {
                _requestedInput = newInput;
            }
            else if (Proj.Power.ProjectorIsCooling)
            {
                _requestedInput = newInput;
                _requestedPower = true;
            }
            else
            {
                _requestedInput = newInput;
                Proj.Power.PowerOn();
            }
        }

        public void SetInput(ProjectorInput input)
        {
            if (input == _currentState) return;
            SendInputCmd(input);
        }

        public void PowerIsUpdated()
        {
            if (!Proj.Power.PowerIsOn && !Proj.Power.ProjectorIsCooling && !Proj.Power.ProjectorIsWarming)
                return;

            if (_requestedPower)
                Proj.Power.PowerOn();

            _requestedPower = false;
        }

        private bool CheckForProjectorMute(int input)
        {
            _requestedInput = null;

            if (input == 0 && Proj.Power.PowerIsOn)
            {
                Proj.MuteOn();
                return true;
            }

            if (input > 0 && Proj.Mute.MuteIsOn)
            {
                Proj.MuteOff();
                return false;
            }

            return false;
        }

        private void SendInputCmd(ProjectorInput input)
        {
            Proj.EnqueueCmd(input.Cmd);
            UpdateState(input);
        }

        protected InputState(EpsonProjector proj, IStateManager<ProjectorInput> stateManager)
            : base(proj, stateManager)
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
            {
                _requestedInput = null;
                return;
            }

            Debug.Console(2, this, "Updating projector input to: {0}", state.Name);
            Proj.Input = GetStateForProjectorInput(this, state);

            if (_requestedInput != null && _requestedInput != state)
                SendInputCmd(_requestedInput);

            Dispose();
        }

        protected static InputState GetStateForProjectorInput(InputState state, ProjectorInput input)
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