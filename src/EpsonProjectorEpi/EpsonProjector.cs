using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;                          
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Config;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.States;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace EpsonProjectorEpi
{
    public class EpsonProjector : TwoWayDisplayBase, IBridgeAdvanced, IBasicVideoMuteWithFeedback
    {
        public const int ProjectorMuteOn = 101;
        public const int ProjectorMuteOff = 102;
        public const int ProjectorMuteToggle = 103;

        private readonly IBasicCommunication _coms;
        private readonly CommandProcessor _commandQueue;
        private CTimer _poll;

        private ProjectorPower _currentPower = ProjectorPower.PowerOff;
        private ProjectorMute _currentMute = ProjectorMute.MuteOff;
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
                {
                    EnqueueCmd(_currentInput.Command);
                    EnqueueCmd(_currentMute.Command);
                }

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

        public ProjectorMute CurrentMute 
        { 
            get { return _currentMute; }
            set
            {
                _currentMute = value;
                Debug.Console(1, this, "Mute set to {0}", _currentMute.Name);

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
        public BoolFeedback MuteIsOnFb { get; private set; }
        public BoolFeedback MuteIsOffFb { get; private set; }
        public IntFeedback CurrentInputValueFeedback { get; set; }

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

            var powerManager = new ResponseStateManager<PowerResponseEnum, ProjectorPower>(Key + "-PowerState", gather);
            var muteManager = new ResponseStateManager<MuteResponseEnum, ProjectorMute>(Key + "-MuteState", gather);
            var inputManager = new ResponseStateManager<InputResponseEnum, ProjectorInput>(Key + "-InputState", gather);
            var serialNumberManager = new SerialNumberStateManager(Key + "-SerialNumber", gather);
            var lampHoursManager = new LampHoursStateManager(Key + "-LampHours", gather);

            WarmupTimer = new CTimer(delegate { }, Timeout.Infinite);
            CooldownTimer = new CTimer(delegate { }, Timeout.Infinite);

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

            muteManager.StateUpdated += (sender, args) =>
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

            CrestronEnvironment.ProgramStatusEventHandler += (args) =>
                {
                    if (args != eProgramStatusEventType.Stopping) 
                        return;

                    _poll.Stop();
                    _poll.Dispose();
                    CommunicationMonitor.Stop();
                };
        }

        private void ConfigureInputs(PropsConfig config)
        {
            foreach (var input in ProjectorInput.GetAll())
            {
                Debug.Console(0, this, "Adding Routing input - {0}", input.Name);

                var newInput = new RoutingInputPort(
                    input.Name,
                    eRoutingSignalType.Video,
                    eRoutingPortConnectionType.BackplaneOnly,
                    input,
                    this) { Port = input.Value };

                InputPorts.Add(newInput);
            }

            if (config.Inputs != null)
            {
                foreach (var inputPort in InputPorts)
                    inputPort.Port = null;

                foreach (var item in config.Inputs)
                    AddInputByNumberFromConfig(item);
            }

            var port = InputPorts.FirstOrDefault();
            if (port == null)
                throw new NullReferenceException("default input port");

            _currentInput = port.Selector as ProjectorInput;
            if (_currentInput == null)
                throw new NullReferenceException("default input");
        }

        private void AddInputByNumberFromConfig(KeyValuePair<string, int> item)
        {
            try
            {
                Debug.Console(1, this, "Attempting to add input routing by Number... | {0} : {1}", item.Key, item.Value);
                var input = ProjectorInput.FromName(item.Key, true);

                var routingPort = InputPorts[input.Name];
                if (routingPort == null)
                    return;

                Debug.Console(1, "Adding input routing by Number... | {0} : {1}", item.Key, item.Value);
                routingPort.Port = input.Value;
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

            Feedbacks.AddRange(new Feedback[]
            {
                PowerIsOnFeedback,
                IsWarmingUpFeedback,
                IsCoolingDownFeedback,
                MuteIsOffFb,
                CurrentInputFeedback,
                CurrentInputValueFeedback,
                SerialNumberFb,
                LampHoursFb,
                MuteIsOnFb,
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
                MuteOn();
                return true;
            }

            if (input == ProjectorMuteOn)
            {
                MuteOn();
                return true;
            }

            if (input == ProjectorMuteOff)
            {
                MuteOff();
                return true;
            }

            if (input != ProjectorMuteToggle) 
                return false;

            MuteToggle();
            return true;
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

        public void MuteOn()
        {
            if (_currentMute == ProjectorMute.MuteOn)
                return;

            if (_currentPower == ProjectorPower.PowerOn)
                EnqueueCmd(ProjectorMute.MuteOn.Command);

            CurrentMute = ProjectorMute.MuteOn;
        }

        public void MuteOff()
        {
            if (_currentMute == ProjectorMute.MuteOff)
                return;

            if (_currentPower == ProjectorPower.PowerOn)
                EnqueueCmd(ProjectorMute.MuteOff.Command);

            CurrentMute = ProjectorMute.MuteOff;
        }

        public void MuteToggle()
        {
            if (_currentMute == ProjectorMute.MuteOff)
                MuteOn();
            else
                MuteOff();
        }

        private void UpdatePollForPowerState()
        {
            if (_poll == null)
            {
                _poll = new CTimer(o => EnqueueCmd(new PowerPollCmd()), 250);
            }

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

        #region IBridgeAdvanced Members

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

        #endregion

        #region IBasicVideoMuteWithFeedback Members

        public BoolFeedback VideoMuteIsOn
        {
            get { return MuteIsOffFb; }
        }

        public void VideoMuteOff()
        {
            MuteOff();
        }

        public void VideoMuteOn()
        {
            MuteOn();
        }

        #endregion

        #region IBasicVideoMute Members

        public void VideoMuteToggle()
        {
            MuteToggle();
        }

        #endregion
    }
}

