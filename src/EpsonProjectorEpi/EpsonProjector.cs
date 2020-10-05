using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;                          
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Config;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Interfaces;
using EpsonProjectorEpi.Mute;
using EpsonProjectorEpi.States;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace EpsonProjectorEpi
{
    public class EpsonProjector : TwoWayDisplayBase, IBridgeAdvanced, IPowerWithWarmingCooling, IBasicVideoMuteWithFeedback
    {
        public const int ProjectorMuteOn = 101;
        public const int ProjectorMuteOff = 102;
        public const int ProjectorMuteToggle = 103;

        private readonly EpsonMute _videoMute;
        private readonly IBasicCommunication _coms;
        private readonly CommandProcessor _commandQueue;
        private CTimer _poll;
        private readonly CTimer _deviceInfoPoll;

        private ProjectorPower _currentPower = ProjectorPower.PowerOff;
        private ProjectorInput _currentInput = ProjectorInput.Dvi;

        private readonly FeedbackCollection<Feedback> _feedbacks = new FeedbackCollection<Feedback>(); 
        public override FeedbackCollection<Feedback> Feedbacks 
        {
            get { return _feedbacks; }
        }

        public ProjectorPower CurrentPower 
        {
            get { return _currentPower; }
            private set
            {
                _currentPower = value;
                Debug.Console(1, this, "Power set to {0}", _currentPower.Name);

                if (_currentPower == ProjectorPower.PowerOn)
                    EnqueueCmd(_currentInput.Command);

                UpdatePollForPowerState();
                Feedbacks.ForEach(fb => fb.FireUpdate());
            }
        }

        public ProjectorInput CurrentInput 
        { 
            get { return _currentInput; }
            private set
            {
                _currentInput = value;
                Debug.Console(1, this, "Input set to {0}", _currentInput.Name);

                Feedbacks.ForEach(fb => fb.FireUpdate());
            }
        }

        private string _serialNumber = string.Empty;
        public string SerialNumber
        {
            get { return _serialNumber; }
        }

        public int LampHours { get; private set; }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public StringFeedback StatusFb { get; private set; }
        public StringFeedback SerialNumberFb { get; private set; }
        public IntFeedback LampHoursFb { get; private set; }
        public IntFeedback CurrentInputValueFeedback { get; private set; }
        public BoolFeedback PowerIsOffFeedback { get; private set; }

        public string ScreenName { get; private set; }

        public EpsonProjector(string key, string name, PropsConfig config, IBasicCommunication coms)
            : base(key, name)
        {
            ScreenName = config.ScreenName;
            WarmupTime = config.WarmupTime == default(int) ? 10000 : config.WarmupTime;
            CooldownTime = config.CooldownTime == default(int) ? 10000 : config.CooldownTime;

            _coms = coms;
            _commandQueue = new CommandProcessor(_coms);
            var gather = new CommunicationGather(_coms, "\x0D:");
            _videoMute = new EpsonMute(this, gather, EnqueueCmd);

            var powerManager = new ResponseStateManager<PowerResponseEnum, ProjectorPower>(Key + "-PowerState", gather);
            var inputManager = new ResponseStateManager<InputResponseEnum, ProjectorInput>(Key + "-InputState", gather);
            var serialNumberManager = new SerialNumberStateManager(Key + "-SerialNumber", gather);
            var lampHoursManager = new LampHoursStateManager(Key + "-LampHours", gather);

            WarmupTimer = new CTimer(delegate { }, Timeout.Infinite);
            CooldownTimer = new CTimer(delegate { }, Timeout.Infinite);
            _deviceInfoPoll = new CTimer(o => _commandQueue.EnqueueCmd(new SerialNumberPollCmd(), new LampPollCmd()), null, Timeout.Infinite);

            ConfigureInputs(config);

            serialNumberManager.StateUpdated += (sender, args) =>
                {
                    _serialNumber = args.CurrentState;
                    SerialNumberFb.FireUpdate();
                };

            lampHoursManager.StateUpdated += (sender, args) =>
                {
                    LampHours = args.CurrentState;
                    LampHoursFb.FireUpdate();
                };

            powerManager.StateUpdated += (sender, args) =>
                {
                    if (_currentPower == args.CurrentState)
                        return;

                    
                    CurrentPower = args.CurrentState;
                };

            inputManager.StateUpdated += (sender, args) =>
                {
                    if (_currentInput == args.CurrentState)
                        return;

                    CurrentInput = args.CurrentState;
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

            CrestronEnvironment.ProgramStatusEventHandler += (args) =>
                {
                    if (args != eProgramStatusEventType.Stopping) 
                        return;

                    _poll.Stop();
                    _poll.Dispose();
                    _deviceInfoPoll.Stop();
                    _deviceInfoPoll.Dispose();
                    CommunicationMonitor.Stop();
                };
        }

        private void ConfigureInputs(PropsConfig config)
        {
            try
            {
                if (config.Inputs != null)
                {
                    foreach (var item in config.Inputs)
                    {
                        Debug.Console(1, this, "Adding input routing by Number... | {0} : {1}", item.Key, item.Value);

                        var input = ProjectorInput.FromName(item.Key, true);

                        var newInput = new RoutingInputPort(
                            input.Name,
                            eRoutingSignalType.Video,
                            eRoutingPortConnectionType.BackplaneOnly,
                            input,
                            this) {Port = item.Value};

                        InputPorts.Add(newInput);
                    }
                }
                else
                {
                    foreach (var input in ProjectorInput.GetAll())
                    {
                        Debug.Console(1, this, "Adding input routing by Number... | {0} : {1}", input.Name, input.Value);

                        var newInput = new RoutingInputPort(
                            input.Name,
                            eRoutingSignalType.Video,
                            eRoutingPortConnectionType.BackplaneOnly,
                            input,
                            this) {Port = input.Value};

                        InputPorts.Add(newInput);
                    }
                }

                var port = InputPorts.FirstOrDefault();
                if (port == null)
                    throw new NullReferenceException("default input port");

                _currentInput = port.Selector as ProjectorInput;
                if (_currentInput == null)
                    throw new NullReferenceException("default input");
            }
            catch (ArgumentException ex)
            {
                Debug.Console(1, this, "Error adding input routing ports : {0}", ex.Message);
                Debug.Console(1, this, "Config value does not match existing inputs - ");
                ProjectorInput
                    .GetAll()
                    .ToList()
                    .ForEach(input => Debug.Console(1, this, input.Name));

                throw;
            }
            catch (Exception ex)
            {
                Debug.Console(1, this, "Error adding input routing ports : {0}", ex.Message);
                throw;
            }
        }

        public override bool CustomActivate()
        {
            Debug.Console(1, this, "Good morning, Dave...");
            base.CustomActivate();

            BuildFeedbacks();
            CommunicationMonitor.Start();
            UpdatePollForPowerState();

            return true;
        }

        private void BuildFeedbacks()
        {
            PowerIsOffFeedback = new BoolFeedback(() => CurrentPower == ProjectorPower.PowerOff);
            SerialNumberFb = new StringFeedback(() => SerialNumber);
            LampHoursFb = new IntFeedback(() => LampHours);
            StatusFb = new StringFeedback(() => CommunicationMonitor.Status.ToString());

            CurrentInputValueFeedback = new IntFeedback(() =>
            {
                if (CurrentPower != ProjectorPower.PowerOn)
                    return 0;

                var result =
                    InputPorts.FirstOrDefault(t => t.Key.Equals(_currentInput.Name, StringComparison.OrdinalIgnoreCase));

                if (result == null || result.Port == null)
                    return 0;

                return Convert.ToInt32(result.Port);
            });

            Feedbacks.AddRange(new Feedback[]
            {
                PowerIsOffFeedback,
                PowerIsOnFeedback,
                IsWarmingUpFeedback,
                IsCoolingDownFeedback,
                CurrentInputFeedback,
                CurrentInputValueFeedback,
                SerialNumberFb,
                LampHoursFb,
                StatusFb
            });
        }

        public void ExecuteSwitchNumeric(int input)
        {
            if (CheckIfValueIsMute(input)) return;

            var result = InputPorts
                .Where(x => x.Port != null)
                .FirstOrDefault(x => Convert.ToInt32(x.Port) == input);

            if (result != null)
                ExecuteSwitch(result.Selector);
        }

        private bool CheckIfValueIsMute(int input)
        {
            if (input == default(int))
            {
                //_videoMute.VideoMuteOn();
                return true;
            }

            if (input == ProjectorMuteOn)
            {
                _videoMute.VideoMuteOn();
                return true;
            }

            if (input == ProjectorMuteOff)
            {
                _videoMute.VideoMuteOff();
                return true;
            }

            if (input != ProjectorMuteToggle) 
                return false;

            _videoMute.VideoMuteToggle();
            return true;
        }

        public void ExecuteSwitch(ProjectorInput input)
        {
            PowerOn();
            _videoMute.VideoMuteOff();

            CheckPowerAndSwitchInput(input);
        }

        public override void ExecuteSwitch(object inputSelector)
        {
            var input = inputSelector as ProjectorInput;
            if (input == null) 
                return;

            PowerOn();
            _videoMute.VideoMuteOff();

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
            if (_commandQueue.Disposed)
                return;

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
            get { return () => _currentPower == ProjectorPower.PowerOn; }
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
                Debug.Console(1, this, "Starting warmup timer to catch up... {0}", WarmupTime / 1000);
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
                Debug.Console(1, this, "Starting cooldown timer to catch up... {0}", CooldownTime / 1000);
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

        private void UpdatePollForPowerState()
        {
            if (_poll == null)
                _poll = new CTimer(o => EnqueueCmd(new PowerPollCmd()), 250);

            if (!CommunicationMonitor.IsOnline)
            {
                _poll.Stop();
                _poll = new CTimer(o => EnqueueCmd(new PowerPollCmd()), null, 250, 20000);

                return;
            }

            if (_currentPower == ProjectorPower.PowerOn)
            {
                _poll.Stop();
                _poll = new CTimer(o =>
                    _commandQueue.EnqueueCmd(
                        new PowerPollCmd(),
                        new SourcePollCmd()), null, 500, 10000);

                _deviceInfoPoll.Reset(15000, 60000);
                return;
            }
            _deviceInfoPoll.Stop();

            if (_currentPower == ProjectorPower.PowerOff)
            {
                _poll.Stop();
                _poll = new CTimer(o => EnqueueCmd(new PowerPollCmd()), null, 250, 10000);
   
                return;
            }

            if (_currentPower == ProjectorPower.Warming)
            {
                _poll.Stop();
                _poll = new CTimer(o => EnqueueCmd(new PowerPollCmd()), null, WarmupTime, 1000);

                return;
            }

            if (_currentPower != ProjectorPower.Cooling) 
                return;

            _poll.Stop();
            _poll = new CTimer(o => EnqueueCmd(new PowerPollCmd()), null, CooldownTime, 1000);
        }

        public void LinkToApi(Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            try
            {
                Debug.Console(1, this, "Attempting to link {0} at {1}", trilist.ID, joinStart);

                var joinMap = new EpsonProjectorJoinMap(joinStart);
                if (bridge != null)
                    bridge.AddJoinMap(Key, joinMap);

                new EpsonProjectorBridge().LinkToApi(this, trilist, joinMap);
            }
            catch (Exception ex)
            {
                Debug.Console(1, this, "Error linking Projector! {0}", ex.Message);
                throw;
            }
        }

        public void VideoMuteToggle()
        {
            _videoMute.VideoMuteToggle();
        }

        public void VideoMuteOn()
        {
            _videoMute.VideoMuteOn();
        }

        public void VideoMuteOff()
        {
            _videoMute.VideoMuteOff();
        }

        public BoolFeedback VideoMuteIsOn
        {
            get { return _videoMute.VideoMuteIsOn; }
        }

        public BoolFeedback VideoMuteIsOff
        {
            get { return _videoMute.VideoMuteIsOff; }
        }
    }
}

