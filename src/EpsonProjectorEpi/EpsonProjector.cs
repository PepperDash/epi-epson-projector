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
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Queries;

namespace EpsonProjectorEpi
{
    public class EpsonProjector : TwoWayDisplayBase, ICommunicationMonitor, IBridge
    {
        readonly IBasicCommunication _coms;
        readonly IPollManager _poll;
        readonly IStatusManager _status;

        public IBasicCommunication Coms { get { return _coms; } }
        public StatusMonitorBase CommunicationMonitor { get; set; }

        public StringFeedback StatusFb { get; private set; }
        public StringFeedback SerialNumberFb { get; private set; }
        public IntFeedback LampHoursFb { get; private set; }
        public BoolFeedback MuteIsOnFb { get; private set; }
        public IntFeedback CurrentInputValueFb { get; set; }

        public string ScreenName { get; set; }

        public EpsonProjector(
            string key, 
            string name, 
            IBasicCommunication coms, 
            IPollManager poll,
            IStatusManager status)
            : base(key, name)
        {
            _coms = coms;
            _poll = poll;
            _status = status;

            AddPostActivationAction(StartPolls);
            AddPostActivationAction(StartCommunicationMonitor);
            AddPostActivationAction(HandleStatusUpdated);
        }

        public override bool CustomActivate()
        {    
            SerialNumberFb = new StringFeedback(() => _status.SerialNumber);
            LampHoursFb = new IntFeedback(() => _status.LampHours);
            MuteIsOnFb = new BoolFeedback(() => _status.MuteStatus == ProjectorMute.MuteOn && _status.PowerStatus == ProjectorPower.PowerOn);
            StatusFb = new StringFeedback(() => CommunicationMonitor.Status.ToString());
            CurrentInputValueFb = new IntFeedback(() => _status.InputStatus.Value);

            _coms.Connect();
            return true;
        }

        void StartCommunicationMonitor()
        {
            CommunicationMonitor.Start();
            CommunicationMonitor.StatusChange += (sender, args) => StatusFb.FireUpdate();
            StatusFb.FireUpdate();
        }

        void StartPolls()
        {
            _poll.Start();
        }

        void HandleStatusUpdated()
        {
            if (_status.PowerStatus != ProjectorPower.PowerOn)
                _status.MuteStatus = ProjectorMute.MuteOff;

            _status.StatusUpdated += (sender, args) => UpdateAllFeedbacks();
        }

        void UpdateAllFeedbacks()
        {
            PowerIsOnFeedback.FireUpdate();
            IsCoolingDownFeedback.FireUpdate();
            IsWarmingUpFeedback.FireUpdate();
            CurrentInputValueFb.FireUpdate();
            CurrentInputFeedback.FireUpdate();
            SerialNumberFb.FireUpdate();
            LampHoursFb.FireUpdate();
            MuteIsOnFb.FireUpdate();
        }

        public void ExecuteSwitch(ProjectorInput input)
        {
            var cmd = input.Cmd;

            new CmdHandler(_coms, cmd).Handle();
            _poll.Start();
        }

        public void ExecuteSwitch(int input)
        {
            ProjectorInput result;
            if (!ProjectorInput.TryFromValue(input, out result)) return;

            new CmdHandler(_coms, result.Cmd).Handle();
            _poll.Start();
        }

        public override void ExecuteSwitch(object inputSelector)
        {
            var cmd = inputSelector as IEpsonCmd;
            if (cmd == null) return;

            new CmdHandler(_coms, cmd).Handle();
            _poll.Start();
        }

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => _status.InputStatus.Name; }
        }

        protected override Func<bool> IsCoolingDownFeedbackFunc
        {
            get { return () => _status.PowerStatus == ProjectorPower.Cooling; }
        }

        protected override Func<bool> IsWarmingUpFeedbackFunc
        {
            get { return () => _status.PowerStatus == ProjectorPower.Warming; }
        }

        protected override Func<bool> PowerIsOnFeedbackFunc
        {
            get { return () => _status.PowerStatus == ProjectorPower.PowerOn || _status.PowerStatus == ProjectorPower.Warming; }
        }

        public override void PowerOff()
        {
            IEpsonCmd cmd = new PowerOffCmd();
            var result = new CmdHandler(_coms, cmd).Handle();

            if (result) _status.PowerStatus = ProjectorPower.Cooling;
            UpdateAllFeedbacks();
        }

        public override void PowerOn()
        {
            IEpsonCmd cmd = new PowerOnCmd();
            var result = new CmdHandler(_coms, cmd).Handle();

            if (result) _status.PowerStatus = ProjectorPower.Warming;
            UpdateAllFeedbacks();
        }

        public override void PowerToggle()
        {
            throw new NotImplementedException();
        }

        public void MuteOn()
        {
            if (!PowerIsOnFeedbackFunc.Invoke()) return;

            IEpsonCmd cmd = new MuteOnCmd();
            var result = new CmdHandler(_coms, cmd).Handle();

            if (result) _status.MuteStatus = ProjectorMute.MuteOn;
            UpdateAllFeedbacks();
        }

        public void MuteOff()
        {
            if (!PowerIsOnFeedbackFunc.Invoke()) return;

            IEpsonCmd cmd = new MuteOffCmd();
            var result = new CmdHandler(_coms, cmd).Handle();

            if (result) _status.MuteStatus = ProjectorMute.MuteOff;
            UpdateAllFeedbacks();
        }

        public void MuteToggle()
        {
            if (!PowerIsOnFeedbackFunc.Invoke()) return;

            if (_status.MuteStatus == ProjectorMute.MuteOff) MuteOn();
            else MuteOff();
        }

        #region IBridge Members

        public void LinkToApi(Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            this.LinkToApiExt(trilist, joinStart, joinMapKey);
        }

        #endregion
    }
}

