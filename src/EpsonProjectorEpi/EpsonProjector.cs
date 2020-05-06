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
    public class EpsonProjector : TwoWayDisplayBase, ICommunicationMonitor, IBridge
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
                Debug.Console(2, this, "Updating Power State : '{0}'", value.Current.Name);
                _power = value;
             
                PowerIsOnFeedback.FireUpdate();
                IsCoolingDownFeedback.FireUpdate();
                IsWarmingUpFeedback.FireUpdate();
                CurrentInputValueFb.FireUpdate();
                CurrentInputFeedback.FireUpdate();
                MuteIsOnFb.FireUpdate();

                _mute.PowerIsUpdated();
                _input.PowerIsUpdated();
            }
        }

        private InputState _input;
        public InputState Input
        {
            get { return _input; }
            set
            {
                Debug.Console(2, this, "Updating Input State : '{0}'", value.Current.Name);
                _input = value;

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
                Debug.Console(2, this, "Updating Mute State : '{0}'", value.Current.Name);
                _mute = value;

                MuteIsOnFb.FireUpdate();
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

        public StringFeedback StatusFb { get; private set; }
        public StringFeedback SerialNumberFb { get; private set; }
        public IntFeedback LampHoursFb { get; private set; }
        public BoolFeedback MuteIsOnFb { get; private set; }
        public IntFeedback CurrentInputValueFb { get; set; }

        public string ScreenName { get; private set; }

        public EpsonProjector(DeviceConfig config)
            : base(config.Key, config.Name)
        {
            var props = PropsConfig.FromDeviceConfig(config);
            ScreenName = props.ScreenName;

            AddPostActivationAction(() => _coms = CommFactory.CreateCommForDevice(config));
            AddPostActivationAction(() => _coms.Connect());
            AddPostActivationAction(() => _commandProcessor = new CmdProcessor(_coms));
            AddPostActivationAction(() => CommunicationMonitor = new GenericCommunicationMonitor(this, _coms, props.Monitor));
            AddPostActivationAction(BuildStates);
            AddPostActivationAction(StartPolls);
            AddPostActivationAction(StartCommunicationMonitor);
        }

        void BuildStates()
        {
            BuildFeedbacks();

            _powerStateManager = new CmdStateManager<ProjectorPower>(Key + "PowerStateManager", _coms);
            _power = new PowerOffState(this);

            _input = new InputStateHdmi(this);
            _inputStateManger = new CmdStateManager<ProjectorInput>(Key + "InputStateManager", _coms);

            _mute = new MuteOffState(this);
            _muteStateManager = new CmdStateManager<ProjectorMute>(Key + "MuteStateManager", _coms);

            _serialNumberStateManager = new SerialNumberStateManager(Key + "SerialNumberManager", _coms);
            _lampHoursStateManager = new LampHoursStateManager(Key + "LampHoursStateManager", _coms);

            SubscribeToStateManagers();

            PowerIsOnFeedback.FireUpdate();
            IsCoolingDownFeedback.FireUpdate();
            IsWarmingUpFeedback.FireUpdate();
            CurrentInputValueFb.FireUpdate();
            CurrentInputFeedback.FireUpdate();
            MuteIsOnFb.FireUpdate();
        }

        void SubscribeToStateManagers()
        {
            _powerStateManager.StateUpdated += (sender, args) =>
                {
                    if (_powerStateManager.State == Power.Current)
                        return;

                    Power = PowerState.GetPowerStateForProjectorPower(Power, _powerStateManager.State);
                };

            _inputStateManger.StateUpdated += (sender, args) =>
                {
                    if (_inputStateManger.State == Input.Current)
                        return;

                    Input = InputState.GetStateForProjectorInput(Input, _inputStateManger.State);
                };

            _muteStateManager.StateUpdated += (sender, args) =>
                {
                    if (_muteStateManager.State == Mute.Current)
                        return;

                    Mute = MuteState.GetMuteStateForProjector(Mute, _muteStateManager.State);
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

        void BuildFeedbacks()
        {
            SerialNumberFb = new StringFeedback(() => SerialNumber);
            LampHoursFb = new IntFeedback(() => LampHours);
            MuteIsOnFb = new BoolFeedback(() => _mute.MuteIsOn);
            StatusFb = new StringFeedback(() => CommunicationMonitor.Status.ToString());
            CurrentInputValueFb = new IntFeedback(() => _input.InputNumber);
        }

        void StartCommunicationMonitor()
        {
            CommunicationMonitor.Start();
            CommunicationMonitor.StatusChange += (sender, args) => StatusFb.FireUpdate();
            StatusFb.FireUpdate();
        }

        void StartPolls()
        {
            _poll = new PollManager(EnqueueCmd, Power);
            _poll.Start();
        }

        public void ExecuteSwitch(ProjectorInput input)
        {
            _input.SetInput(input);
            _poll.Start();
        }

        public void ExecuteSwitch(int input)
        {
            _input.SetInput(input);
            _poll.Start();
        }

        public override void ExecuteSwitch(object inputSelector)
        {
            var input = inputSelector as ProjectorInput;
            if (input == null) return;

            _input.SetInput(input);
            _poll.Start();
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

