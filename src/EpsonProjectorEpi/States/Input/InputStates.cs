using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.States.Input
{
    public class InputStateHdmi : InputState
    {
        public InputStateHdmi(InputState state)
            : this(state.Proj)
        {

        }

        public InputStateHdmi(EpsonProjector proj)
            : base(proj)
        {
            _currentState = ProjectorInput.Hdmi;
        }   
    }

    public class InputStateDvi : InputState
    {
        public InputStateDvi(InputState state)
            : this(state.Proj)
        {

        }

        public InputStateDvi(EpsonProjector proj)
            : base(proj)
        {
            _currentState = ProjectorInput.Dvi;
        }
    }

    public class InputStateVideo : InputState
    {
        public InputStateVideo(InputState state)
            : this(state.Proj)
        {

        }

        public InputStateVideo(EpsonProjector proj)
            : base(proj)
        {
            _currentState = ProjectorInput.Video;
        }
    }

    public class InputStateComputer : InputState
    {
        public InputStateComputer(InputState state)
            : this(state.Proj)
        {

        }

        public InputStateComputer(EpsonProjector proj)
            : base(proj)
        {
            _currentState = ProjectorInput.Computer;
        }
    }
}