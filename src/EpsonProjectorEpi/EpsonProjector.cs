using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       				// For Basic SIMPL#Pro classes
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Monitoring;
using PepperDash.Essentials.Core.Config;
using EpsonProjectorEpi.Config;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Queries;
using EpsonProjectorEpi.States;

namespace EpsonProjectorEpi
{
    public class EpsonProjector : TwoWayDisplayBase, IBridgeAdvanced
    {
        private readonly IBasicCommunication _coms;
        private readonly CommandProcessor _commandQueue;
        private CTimer _poll;

        private readonly IStateManager<ProjectorPower> _powerManager;
        private readonly IStateManager<ProjectorMute> _muteManager;
        private readonly IStateManager<ProjectorInput> _inputManager;
        private readonly IStateManager<string> _serialNumberManager;
        private readonly IStateManager<int> _lampHoursManager;

        private ProjectorPower _currentPower = ProjectorPower.PowerOff;
        private ProjectorMute _currentMute = ProjectorMute.MuteOff;
        private ProjectorInput _currentInput = ProjectorInput.Dvi;

        public ProjectorPower CurrentPower 
        {
            get { return _currentPower; }
            private set
            {
                _currentPower = value;
                Debug.Console(1, this, "Power set to {0}", _currentPower.Name);

                if (_currentPower == ProjectorPower.PowerOn)
                    CheckPowerAndSwitchInput(_currentInput);

                if (_currentPower == ProjectorPower.PowerOff || _currentPower == ProjectorPower.Warming)
                    CurrentMute = ProjectorMute.MuteOff;

                UpdatePollForPowerState();

                PowerIsOnFeedback.FireUpdate();
                IsCoolingDownFeedback.FireUpdate();
                IsWarmingUpFeedback.FireUpdate();
                CurrentInputFeedback.FireUpdate();
                CurrentInputValueFeedback.FireUpdate();
                MuteIsOnFb.FireUpdate();
                MuteIsOffFb.FireUpdate();
            }
        }

        public ProjectorInput CurrentInput 
        { 
            get { return _currentInput; }
            private set
            {
                _currentInput = value;
                Debug.Console(1, this, "Input set to {0}", _currentInput.Name);

                CurrentInputFeedback.FireUpdate();
                CurrentInputValueFeedback.FireUpdate();
            }
        }

        public ProjectorMute CurrentMute 
        { 
            get { return _currentMute; }
            set
            {
                _currentMute = value;
                Debug.Console(1, this, "Mute set to {0}", _currentMute.Name);

                MuteIsOnFb.FireUpdate();
                MuteIsOffFb.FireUpdate();
            }
        }

        private string _serialNumber = string.Empty;
        public string SerialNumber
        {
            get { return _serialNumber; }
        }

        private int _lampHours;
        public int LampHours
        {
            get { return _lampHours; }
        }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public StringFeedback StatusFb { get; private set; }
        public StringFeedback SerialNumberFb { get; private set; }
        public IntFeedback LampHoursFb { get; private set; }
        public BoolFeedback MuteIsOnFb { get; private set; }
        public BoolFeedback MuteIsOffFb { get; private set; }
        public IntFeedback CurrentInputValueFeedback { get; set; }

        public string ScreenName { get; private set; }

        public EpsonProjector(string key, string name, PropsConfig config, IBasicCommunication coms)
            : base(key, name)
        {
            ScreenName = config.ScreenName;
            WarmupTime = 10000;
            CooldownTime = 10000;

            _coms = coms;
            _commandQueue = new CommandProcessor(_coms);

            _powerManager = new ResponseStateManager<PowerResponseEnum, ProjectorPower>(Key + "-PowerState", _coms);
            _muteManager = new ResponseStateManager<MuteResponseEnum, ProjectorMute>(Key + "-MuteState", _coms);
            _inputManager = new ResponseStateManager<InputResponseEnum, ProjectorInput>(Key + "-InputState", _coms);
            _serialNumberManager = new SerialNumberStateManager(Key + "-SerialNumber", _coms);
            _lampHoursManager = new LampHoursStateManager(Key + "-LampHours", _coms);

            WarmupTimer = new CTimer(delegate { }, Timeout.Infinite);
            CooldownTimer = new CTimer(delegate { }, Timeout.Infinite);

            _serialNumberManager.StateUpdated += (sender, args) =>
                {
                    _serialNumber = args.CurrentState;
                    SerialNumberFb.FireUpdate();
                };

            _lampHoursManager.StateUpdated += (sender, args) =>
                {
                    _lampHours = args.CurrentState;
                    LampHoursFb.FireUpdate();
                };

            _powerManager.StateUpdated += (sender, args) =>
                {
                    if (_currentPower == args.CurrentState)
                        return;

                    CurrentPower = args.CurrentState;
                };

            _inputManager.StateUpdated += (sender, args) =>
                {
                    if (_currentInput == args.CurrentState)
                        return;

                    CurrentInput = args.CurrentState;
                };

            _muteManager.StateUpdated += (sender, args) =>
                {
                    if (_currentMute == args.CurrentState)
                        return;

                    CurrentMute = args.CurrentState;
                };

            AddPreActivationAction(() =>
                {
                    if (config.Monitor == null)
                        config.Monitor = new CommunicationMonitorConfig() { PollString = new PowerPollCmd().CmdString };

                    CommunicationMonitor = new GenericCommunicationMonitor(this, _coms, config.Monitor);
                    CommunicationMonitor.StatusChange += (sender, args) =>
                        {
                            if (!CommunicationMonitor.IsOnline)
                                CurrentPower = ProjectorPower.PowerOff;
                            else
                                UpdatePollForPowerState();
                        };
                });

            AddPostActivationAction(() => CurrentPower = ProjectorPower.PowerOff);

            CrestronEnvironment.ProgramStatusEventHandler += (args) =>
                {
                    if (args == eProgramStatusEventType.Stopping)
                    {
                        _poll.Stop();
                        _poll.Dispose();
                        CommunicationMonitor.Stop();
                    }
                };
        }

        public override bool CustomActivate()
        {
            Debug.Console(1, this, "Good morning, Dave...");
            base.CustomActivate();

            BuildFeedbacks();
            CommunicationMonitor.Start();

            return true;
        }

        private void BuildFeedbacks()
        {
            SerialNumberFb = new StringFeedback(() => SerialNumber);
            LampHoursFb = new IntFeedback(() => LampHours);
            MuteIsOnFb = new BoolFeedback(() => _currentMute == ProjectorMute.MuteOn);
            MuteIsOffFb = new BoolFeedback(() => !MuteIsOnFb.BoolValue);
            StatusFb = new StringFeedback(() => CommunicationMonitor.Status.ToString());
            CurrentInputValueFeedback = new IntFeedback(() => CurrentPower == ProjectorPower.PowerOn ? _currentInput.Value : 0);
        }

        public void ExecuteSwitchNumeric(int input)
        {
            if (input == 0)
            {
                MuteOn();
                return;
            }

            ProjectorInput result;
            if (ProjectorInput.TryFromValue(input, out result))
                ExecuteSwitch(result);
        }

        public void ExecuteSwitch(ProjectorInput input)
        {
            PowerOn();
            MuteOff();

            CheckPowerAndSwitchInput(input);
        }

        public override void ExecuteSwitch(object inputSelector)
        {
            var input = inputSelector as ProjectorInput;
            if (input == null) 
                return;

            PowerOn();
            MuteOff();

            CheckPowerAndSwitchInput(input);
        }

        private void CheckPowerAndSwitchInput(ProjectorInput input)
        {
            if (_currentPower == ProjectorPower.PowerOn)
            {
                EnqueueCmd(input.Command);
                CurrentInput = input;
            }
            else
            {
                _currentInput = input;
            } 
        }

        public void EnqueueCmd(IEpsonCmd cmd)
        {
            _commandQueue.EnqueueCmd(cmd);
        }

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => _currentPower == ProjectorPower.PowerOn ? 
                _currentInput.Name : "None"; }
        }

        protected override Func<bool> IsCoolingDownFeedbackFunc
        {
            get { return () => _currentPower == ProjectorPower.Cooling; }
        }

        protected override Func<bool> IsWarmingUpFeedbackFunc
        {
            get { return () => _currentPower == ProjectorPower.Warming; }
        }

        protected override Func<bool> PowerIsOnFeedbackFunc
        {
            get { return () => _currentPower == ProjectorPower.Warming || _currentPower == ProjectorPower.PowerOn; }
        }

        public override void PowerOff()
        {
            if (_currentPower == ProjectorPower.PowerOff || _currentPower == ProjectorPower.Cooling || !CommunicationMonitor.IsOnline)
                return;

            if (_currentPower == ProjectorPower.PowerOn)
            {
                EnqueueCmd(new PowerOffCmd());
                CurrentPower = ProjectorPower.Cooling;
            }
            else
            {
                CooldownTimer.Stop();
                WarmupTimer = new CTimer(o => PowerOff(), WarmupTime);
            }
        }

        public override void PowerOn()
        {
            if (_currentPower == ProjectorPower.PowerOn || _currentPower == ProjectorPower.Warming || !CommunicationMonitor.IsOnline)
                return;

            if (_currentPower == ProjectorPower.PowerOff)
            {
                EnqueueCmd(new PowerOnCmd());
                CurrentPower = ProjectorPower.Warming;
            }
            else
            {
                WarmupTimer.Stop();
                CooldownTimer = new CTimer(o => PowerOn(), CooldownTime);
            }
        }

        public override void PowerToggle()
        {
            if (_currentPower == ProjectorPower.PowerOn || _currentPower == ProjectorPower.Warming)
                PowerOff();
            else
                PowerOn();
        }

        public void MuteOn()
        {
            if (_currentPower != ProjectorPower.PowerOn || _currentMute == ProjectorMute.MuteOn)
                return;

            EnqueueCmd(ProjectorMute.MuteOn.Command);
            CurrentMute = ProjectorMute.MuteOn;
        }

        public void MuteOff()
        {
            if (_currentPower != ProjectorPower.PowerOn || _currentMute == ProjectorMute.MuteOff)
                return;

            EnqueueCmd(ProjectorMute.MuteOff.Command);
            CurrentMute = ProjectorMute.MuteOff;
        }

        public void MuteToggle()
        {
            if (_currentPower != ProjectorPower.PowerOn)
                return;

            if (_currentMute == ProjectorMute.MuteOff)
                MuteOn();
            else
                MuteOff();
        }

        private void UpdatePollForPowerState()
        {
            if (_poll == null)
            {
                _poll = new CTimer(o =>
                    {
                        EnqueueCmd(new PowerPollCmd());
                    }, 250);
            }

            if (!CommunicationMonitor.IsOnline)
            {
                _poll.Stop();
                _poll = new CTimer(o =>
                {
                    EnqueueCmd(new PowerPollCmd());
                }, null, 250, 20000);

                return;
            }

            if (_currentPower == ProjectorPower.PowerOn)
            {
                _poll.Stop();
                _poll = new CTimer(o =>
                    {
                        EnqueueCmd(new PowerPollCmd());
                        EnqueueCmd(new SourcePollCmd());
                        EnqueueCmd(new MutePollCmd());
                        EnqueueCmd(new SerialNumberPollCmd());
                        EnqueueCmd(new LampPollCmd());
                    }, null, 250, 10000);

                return;
            }

            if (_currentPower == ProjectorPower.PowerOff)
            {
                _poll.Stop();
                _poll = new CTimer(o =>
                    {
                        EnqueueCmd(new PowerPollCmd());
                    }, null, 250, 10000);

                return;
            }

            if (_currentPower == ProjectorPower.Warming)
            {
                _poll.Stop();
                _poll = new CTimer(o =>
                {
                    EnqueueCmd(new PowerPollCmd());
                }, null, WarmupTime, 1000);

                return;
            }

            if (_currentPower == ProjectorPower.Cooling)
            {
                _poll.Stop();
                _poll = new CTimer(o =>
                {
                    EnqueueCmd(new PowerPollCmd());
                }, null, CooldownTime, 1000);

                return;
            }
        }

        #region IBridgeAdvanced Members

        public void LinkToApi(Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            Debug.Console(1, this, "Attempting to link {0} at {1}", trilist.ID, joinStart);

            var joinMap = new EpsonProjectorJoinMap(joinStart);
            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(1, this, "Apparently you can't use an old bridge with a new join map");
            }
            
            new EpsonProjectorBridge().LinkToApi(this, trilist, joinMap);
        }

        #endregion
    }
}

