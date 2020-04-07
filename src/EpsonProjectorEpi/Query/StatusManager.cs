using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Queries
{
    public class StatusManager : IStatusManager, IKeyed
    {
        readonly string _key;
        readonly CommunicationGather _gather;
        readonly IBasicCommunication _coms;

        public event EventHandler StatusUpdated;
        public ProjectorPower PowerStatus { get; set; }
        public ProjectorInput InputStatus { get; set; }
        public ProjectorMute MuteStatus { get; set; }

        public string SerialNumber { get; private set; }
        public int LampHours { get; private set; }

        public StatusManager(string key, IBasicCommunication coms)
        {
            _key = key;
            _coms = coms;
            _gather = new CommunicationGather(coms, "\x0D:");
            _gather.LineReceived += HandleLineReceived;

            PowerStatus = ProjectorPower.PowerOff;
            InputStatus = ProjectorInput.Hdmi;
            MuteStatus = ProjectorMute.MuteOff;
        }

        void HandleLineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            if (String.IsNullOrEmpty(e.Text)) return;
            Debug.Console(2, _coms, "Line Received: {0}", e.Text);
            ProcessData(e.Text);
        }

        void ProcessData(string data)
        {
            if (data.Contains(ProjectorPower.SearchString))
            {
                var result = new ResponseProcessor<ProjectorPower>(_coms.Key, data).Handle();
                if (result == null) return;

                PowerStatus = result;
                Debug.Console(2, this, "Power status set to {0}", PowerStatus.Name);
                OnStatusUpdated();
                return;
            }

            if (data.Contains(ProjectorInput.SearchString))
            {
                var result = new ResponseProcessor<ProjectorInput>(_coms.Key, data).Handle();
                if (result == null) return;

                InputStatus = result;
                Debug.Console(2, this, "Input status set to {0}", InputStatus.Name);
                OnStatusUpdated();
                return;
            }

            if (data.Contains(ProjectorMute.SearchString))
            {
                var result = new ResponseProcessor<ProjectorMute>(_coms.Key, data).Handle();
                if (result == null) return;

                MuteStatus = result;
                Debug.Console(2, this, "Mute status set to {0}", MuteStatus.Name);
                OnStatusUpdated();
                return;
            }

            if (data.Contains("LAMP"))
            {
                LampHours = new LampResponseProcessor(_coms.Key, data).Handle();
                OnStatusUpdated();
                return;
            }

            if (data.Contains("SNO"))
            {
                SerialNumber = new SerialNumberResponseProcessor(_coms.Key, data).Handle();
                OnStatusUpdated();
                return;
            }
        }

        void OnStatusUpdated()
        {
            var handler = StatusUpdated;
            if (handler == null) return;

            handler.Invoke(this, EventArgs.Empty);
        }

        #region IKeyed Members

        public string Key
        {
            get { return _key; }
        }

        #endregion
    }
}