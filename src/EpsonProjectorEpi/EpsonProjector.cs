using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       				// For Basic SIMPL#Pro classes
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Monitoring;
using PepperDash.Essentials.Bridges;
using PepperDash.Essentials.Core.Config;
using EpsonProjectorEpi.Config;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Queries;
using EpsonProjectorEpi.Polling;
using EpsonProjectorEpi.States;
using EpsonProjectorEpi.States.Power;
using EpsonProjectorEpi.States.Input;
using EpsonProjectorEpi.States.Mute;

namespace EpsonProjectorEpi
{
    public class EpsonProjector: TwoWayDisplayBase, ICommunicationMonitor, IBridge
    {
        private IBasicCommunication _coms;
        private IPollManager _poll;
        private CmdProcessor _commandProcessor;

        private IStateManager<ProjectorPower> _powerStateManager;
        private IStateManager<ProjectorInput> _inputStateManger;
        private IStateManager<ProjectorMute> _muteStateManager;
        private IStateManager<string> _serialNumberStateManager;
        private IStateManager<int> _lampHoursStateManager;

        private PowerState _power;
        public PowerState Power
        {
            get { return _power; }
            set
            {
                Debug.Console(0, this, "Updating Power State...");
                _power = value;
                Debug.Console(0, this, "Power State : '{0}'", _power.Current.Name);
                Debug.Console(0, this, "Input State : '{0}'", _input.Current.Name);
                Debug.Console(0, this, "Mute State : '{0}'", _mute.Current.Name);

                CheckIfPowerIsWarmingUp();
                CheckIfPowerIsCoolingDown();

                PowerIsOnFeedback.FireUpdate();
                PowerIsOffFb.FireUpdate();
                IsCoolingDownFeedback.FireUpdate();
                IsWarmingUpFeedback.FireUpdate();
                CurrentInputValueFb.FireUpdate();
                CurrentInputFeedback.FireUpdate();
                MuteIsOnFb.FireUpdate();
                MuteIsOffFb.FireUpdate();
            }
        }

        private InputState _input;
        public InputState Input
        {
            get { return _input; }
            set
            {
                Debug.Console(0, this, "Updating Input State...");
                _input = value;
                Debug.Console(0, this, "Power State : '{0}'", _power.Current.Name);
                Debug.Console(0, this, "Input State : '{0}'", _input.Current.Name);
                Debug.Console(0, this, "Mute State : '{0}'", _mute.Current.Name);

                CurrentInputValueFb.FireUpdate();
                CurrentInputFeedback.FireUpdate();
            }
        }

        private MuteState _mute;
        public MuteState Mute
        {
            get { return _mute; }
            set
            {
                Debug.Console(0, this, "Updating Mute State...");
                _mute = value;
                Debug.Console(0, this, "Power State : '{0}'", _power.Current.Name);
                Debug.Console(0, this, "Input State : '{0}'", _input.Current.Name);
                Debug.Console(0, this, "Mute State : '{0}'", _mute.Current.Name);

                MuteIsOnFb.FireUpdate();
                MuteIsOffFb.FireUpdate();
            }
        }

        public string SerialNumber
        {
            get { return _serialNumberStateManager.State; }
        }

        public int LampHours
        {
            get { return _lampHoursStateManager.State; }
        }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public BoolFeedback PowerIsOffFb { get; private set; }
        public StringFeedback StatusFb { get; private set; }
        public StringFeedback SerialNumberFb { get; private set; }
        public IntFeedback LampHoursFb { get; private set; }
        public BoolFeedback MuteIsOnFb { get; private set; }
        public BoolFeedback MuteIsOffFb { get; private set; }
        public IntFeedback CurrentInputValueFb { get; set; }

        public string ScreenName { get; private set; }

        public EpsonProjector(string key, string name, PropsConfig config, IBasicCommunication coms)
            : base(key, name)
        {
            ScreenName = config.ScreenName;
            WarmupTime = 30000;
            CooldownTime = 30000;

            _coms = coms;

            if (config.Monitor == null)
                config.Monitor = new CommunicationMonitorConfig();

            CommunicationMonitor = new GenericCommunicationMonitor(this, _coms, config.Monitor);

            AddPostActivationAction(BuildStates);
            AddPostActivationAction(StartPolls);
            AddPostActivationAction(StartCommunicationMonitor);
        }

        public override bool CustomActivate()
        {
            Debug.Console(2, this, "Good morning, Dave...");
            base.CustomActivate();

            _commandProcessor = new CmdProcessor(_coms);

            return true;
        }

        private void BuildStates()
        {
            BuildFeedbacks();

            _power = new PowerOffState(this);
            _powerStateManager = new CmdStateManager<ProjectorPower>(Key + "PowerStateManager", _coms);   

            _input = new InputStateHdmi(this);
            _inputStateManger = new CmdStateManager<ProjectorInput>(Key + "InputStateManager", _coms);

            _mute = new MuteOffState(this);
            _muteStateManager = new CmdStateManager<ProjectorMute>(Key + "MuteStateManager", _coms);

            _serialNumberStateManager = new SerialNumberStateManager(Key + "SerialNumberManager", _coms);
            _lampHoursStateManager = new LampHoursStateManager(Key + "LampHoursStateManager", _coms);

            SubscribeToStateManagers();

            PowerIsOnFeedback.FireUpdate();
            PowerIsOffFb.FireUpdate();
            IsCoolingDownFeedback.FireUpdate();
            IsWarmingUpFeedback.FireUpdate();
            CurrentInputValueFb.FireUpdate();
            CurrentInputFeedback.FireUpdate();
            MuteIsOnFb.FireUpdate();
            MuteIsOffFb.FireUpdate();
        }

        private void SubscribeToStateManagers()
        {
            _powerStateManager.StateUpdated += (sender, args) =>
                {
                    if (_powerStateManager.State == Power.Current)
                    {
                        Power.Confirmed = true;
                        return;
                    }

                    if (Power.Confirmed)
                    {
                        Power = PowerState.GetPowerStateForProjectorPower(_power, _powerStateManager.State);
                        Power.Confirmed = true;
                    }
                    else
                    {
                        Power.Confirmed = true;
                        EnqueueCmd(Power.Current.Cmd);
                    }
                };

            _inputStateManger.StateUpdated += (sender, args) =>
                {
                    if (_inputStateManger.State == Input.Current)
                    {
                        Input.Confirmed = true;
                        return;
                    }

                    if (Input.Confirmed)
                    {
                        Input = InputState.GetStateForProjectorInput(_input, _inputStateManger.State);
                        Input.Confirmed = true;
                    }
                    else
                    {
                        Input.Confirmed = true;
                        EnqueueCmd(Input.Current.Cmd);
                    }
                };

            _muteStateManager.StateUpdated += (sender, args) =>
                {
                    if (_muteStateManager.State == Mute.Current)
                    {
                        Mute.Confirmed = true;
                        return;
                    }

                    if (Mute.Confirmed)
                    {
                        Mute = MuteState.GetMuteStateForProjector(_mute, _muteStateManager.State);
                        Mute.Confirmed = true;
                    }
                    else
                    {
                        Mute.Confirmed = true;
                        EnqueueCmd(Mute.Current.Cmd);
                    }
                };

            _serialNumberStateManager.StateUpdated += (sender, args) =>
                {
                    Debug.Console(2, this, "Serial Number Updated : '{0}'", _serialNumberStateManager.State);
                    SerialNumberFb.FireUpdate();
                };

            _lampHoursStateManager.StateUpdated += (sender, args) =>
                {
                    Debug.Console(2, this, "Lamp Hours Updated : '{0}'", _lampHoursStateManager.State);
                    LampHoursFb.FireUpdate();
                };
        }

        private void BuildFeedbacks()
        {
            PowerIsOffFb = new BoolFeedback(() => !_power.PowerIsOn && !_power.ProjectorIsWarming);
            SerialNumberFb = new StringFeedback(() => SerialNumber);
            LampHoursFb = new IntFeedback(() => LampHours);
            MuteIsOnFb = new BoolFeedback(() => _mute.MuteIsOn);
            MuteIsOffFb = new BoolFeedback(() => !_mute.MuteIsOn);
            StatusFb = new StringFeedback(() => CommunicationMonitor.Status.ToString());
            CurrentInputValueFb = new IntFeedback(() => _power.PowerIsOn ? _input.InputNumber : 0);
        }

        private void BuildRoutingInputs()
        {
            foreach (var input in ProjectorInput.GetAll())
            {
                var inputActual = input;
                Debug.Console(0, this, "Adding Routing input - {0}", inputActual.Name);

                var newInput = new RoutingInputPort(
                        inputActual.Name,
                        eRoutingSignalType.Video,
                        eRoutingPortConnectionType.BackplaneOnly,
                        inputActual,
                        this);

                InputPorts.Add(newInput);
            }
        }

        private void StartCommunicationMonitor()
        {
            CommunicationMonitor.Start();
            CommunicationMonitor.StatusChange += (sender, args) => StatusFb.FireUpdate();
            StatusFb.FireUpdate();
        }

        private void StartPolls()
        {
            _poll = new PollManager(EnqueueCmd, Power);
            _poll.Start();
        }

        private void CheckIfPowerIsWarmingUp()
        {
            if (!_power.ProjectorIsWarming)
                return;

            WarmupTimer = new CTimer(o =>
            {
                Power = PowerState.GetPowerStateForProjectorPower(_power, ProjectorPower.PowerOn);
                _poll.Start();
                WarmupTimer.Dispose();
            }, WarmupTime);
        }

        private void CheckIfPowerIsCoolingDown()
        {
            if (!_power.ProjectorIsCooling)
                return;

            Mute = MuteState.GetMuteStateForProjector(_mute, ProjectorMute.MuteOff);
            CooldownTimer = new CTimer(o =>
            {
                Power = PowerState.GetPowerStateForProjectorPower(_power, ProjectorPower.PowerOff);
                _poll.Start();
                CooldownTimer.Dispose();
            }, CooldownTime);
        }

        public void ExecuteSwitch(ProjectorInput input)
        {
            _input.SetInput(input);
        }

        public void ExecuteSwitch(int input)
        {
            _input.SetInput(input);
        }

        public override void ExecuteSwitch(object inputSelector)
        {
            var input = inputSelector as ProjectorInput;
            if (input == null) return;

            _input.SetInput(input);
        }

        public void EnqueueCmd(IEpsonCmd cmd)
        {
            _commandProcessor.EnqueueCmd(cmd);
        }

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => _input.InputName; }
        }

        protected override Func<bool> IsCoolingDownFeedbackFunc
        {
            get { return () => _power.ProjectorIsCooling;  }
        }

        protected override Func<bool> IsWarmingUpFeedbackFunc
        {
            get { return () => _power.ProjectorIsWarming; }
        }

        protected override Func<bool> PowerIsOnFeedbackFunc
        {
            get { return () => _power.PowerIsOn || _power.ProjectorIsWarming; }
        }

        public override void PowerOff()
        {
            _power.PowerOff();
        }

        public override void PowerOn()
        {
            _power.PowerOn();
        }

        public override void PowerToggle()
        {
            _power.PowerToggle();
        }

        public void MuteOn()
        {
            _mute.MuteOn();
        }

        public void MuteOff()
        {
            _mute.MuteOff();
        }

        public void MuteToggle()
        {
            _mute.MuteToggle();
        }

        #region IBridge Members

        public void LinkToApi(Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            this.LinkToApiExt(trilist, joinStart, joinMapKey);
        }

        #endregion
    }
}

