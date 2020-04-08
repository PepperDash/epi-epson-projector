using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using EpsonProjectorEpi.Queries;
using EpsonProjectorEpi.Enums;
using PepperDash.Essentials.Core;

namespace EpsonProjectorEpi.Commands
{
    public class PollManager : IDisposable, IPollManager
    {
        public bool Disposed { get; private set; }

        readonly CTimer _timer;
        readonly IStatusManager _status;

        public PollManager(IBasicCommunication coms, IStatusManager status)
        {
            _timer = new CTimer(TimerCallback, coms, Timeout.Infinite, 10000);
            _status = status;
            CrestronEnvironment.ProgramStatusEventHandler += eventType =>
                {
                    switch (eventType)
                    {
                        case eProgramStatusEventType.Stopping:
                            Dispose();
                            break;
                    }
                };
        }

        void TimerCallback(object obj)
        {
            var coms = obj as IBasicCommunication;
            Poll(coms);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!Disposed)
                Disposed = true;

            Dispose(Disposed);
        }

        #endregion

        private void Dispose(bool dispose)
        {
            _timer.Dispose();
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        public void Start()
        {
            _timer.Reset(0, 15000);
        }

        void Poll(IBasicCommunication coms)
        {
            using (var wh = new CEvent())
            {
                new CmdHandler(coms, new PowerPollCmd()).Handle();
                wh.Wait(100);

                if (_status.PowerStatus == ProjectorPower.PowerOff 
                    || _status.PowerStatus == ProjectorPower.Standby 
                    || _status.PowerStatus == ProjectorPower.AbnormalityStandby)
                    return;

                new CmdHandler(coms, new SourcePollCmd()).Handle();
                wh.Wait(100);

                new CmdHandler(coms, new LampPollCmd()).Handle();
                wh.Wait(100);

                new CmdHandler(coms, new MutePollCmd()).Handle();
                wh.Wait(100);
            };
        }
    }
}