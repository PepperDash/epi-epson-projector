using System;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Interfaces;
using EpsonProjectorEpi.States;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace EpsonProjectorEpi.Mute
{
    public class EpsonMute : IBasicVideoMuteWithFeedback, IKeyed
    {
        private readonly IEpsonCmd _pollCmd = new MutePollCmd();

        private readonly IPowerWithWarmingCooling _power;
        private readonly Action<IEpsonCmd> _enqueue;
        private readonly CTimer _poll;

        private ProjectorMute _current = ProjectorMute.MuteOff;
        private ProjectorMute _requested;

        public EpsonMute(IPowerWithWarmingCooling power, CommunicationGather gather, Action<IEpsonCmd> enqueue)
        {
            _enqueue = enqueue;
            _power = power;
            _poll = new CTimer(o =>
            {
                if (!_power.PowerIsOnFeedback.BoolValue)
                    return;

                _enqueue(_pollCmd);
            }, null, 500, 10000);

            Key = _power.Key + "-MuteState";
            var muteManager = new ResponseStateManager<MuteResponseEnum, ProjectorMute>(Key, gather);

            muteManager.StateUpdated += MuteManagerOnStateUpdated;
            _power.PowerIsOnFeedback.OutputChange += PowerOnOutputChange;

            VideoMuteIsOn = new BoolFeedback("VideoMuteOn", () =>
                    _requested == null ? _current == ProjectorMute.MuteOn : _requested == ProjectorMute.MuteOn
                );

            VideoMuteIsOff = new BoolFeedback("VideoMuteOff", () =>
                    _requested == null ? _current == ProjectorMute.MuteOff : _requested == ProjectorMute.MuteOff
                );

            VideoMuteIsOn.OutputChange += PrintOutputChange;
            VideoMuteIsOff.OutputChange += PrintOutputChange;

            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type != eProgramStatusEventType.Stopping)
                    return;

                _poll.Stop();
                _poll.Dispose();
            };
        }

        private void PrintOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            var keyed = sender as IKeyed;
            if (keyed == null)
                return;

            if (sender is BoolFeedback)
                Debug.Console(1, this, "{0} Updated : {1}", keyed.Key, feedbackEventArgs.BoolValue);

            if (sender is IntFeedback)
                Debug.Console(1, this, "{0} Updated : {1}", keyed.Key, feedbackEventArgs.IntValue);

            if (sender is StringFeedback)
                Debug.Console(1, this, "{0} Updated : {1}", keyed.Key, feedbackEventArgs.StringValue);
        }

        private void PowerOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            if (feedbackEventArgs.BoolValue)
                _poll.Reset(100, 1000);
        }

        private void MuteManagerOnStateUpdated(object sender, StateUpdatedEventArgs<ProjectorMute> stateUpdatedEventArgs)
        {
            if (!_power.PowerIsOnFeedback.BoolValue)
                return;

            if (_requested == null)
            {
                ProcessUpdatedState(stateUpdatedEventArgs.CurrentState);
                return;
            }

            if (_requested != stateUpdatedEventArgs.CurrentState)
            {
                Debug.Console(1, this, "Received State: {0}, Requeuested State: {1}", stateUpdatedEventArgs.CurrentState.Name, _requested.Name);
                _enqueue(_requested.Command);
            }
            else
                ProcessUpdatedState(stateUpdatedEventArgs.CurrentState);
        }

        private void ProcessUpdatedState(ProjectorMute state)
        {
            if (_current != state)
            {
                _current = state;
                _requested = null;
                VideoMuteIsOn.FireUpdate();
                VideoMuteIsOff.FireUpdate();
            }
            
            _poll.Reset(10000);
        }

        public void VideoMuteToggle()
        {
            if (_requested != null)
                ProcessToggleForRequested();
            else
                ProcessToggleForCurrent();
        }

        private void ProcessToggleForRequested()
        {
            if (_requested == ProjectorMute.MuteOn)
                VideoMuteOff();
            else
                VideoMuteOn();
        }

        private void ProcessToggleForCurrent()
        {
            if (_current == ProjectorMute.MuteOn)
                VideoMuteOff();
            else
                VideoMuteOn();
        }
        public void VideoMuteOn()
        {
            _requested = ProjectorMute.MuteOn;
            VideoMuteIsOn.FireUpdate();
            VideoMuteIsOff.FireUpdate();
            _poll.Reset(100, 1000);
        }

        public void VideoMuteOff()
        {
            _requested = ProjectorMute.MuteOff;
            VideoMuteIsOn.FireUpdate();
            VideoMuteIsOff.FireUpdate();
            _poll.Reset(100, 1000);
        }

        public BoolFeedback VideoMuteIsOn { get; private set; }
        public BoolFeedback VideoMuteIsOff { get; private set; }
        public string Key { get; private set; }
    }
}