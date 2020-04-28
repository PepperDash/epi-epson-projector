using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Queries;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.States.Power;

namespace EpsonProjectorEpi.Polling
{
    public class PollManager : IPollManager
    {
        private readonly Action<IEpsonCmd> _enqueueCmd;
        public bool Disposed { get; private set; }

        readonly CTimer _timer;
        readonly PowerState _power;

        public PollManager(Action<IEpsonCmd> enqueueAction, PowerState power)
        {
            _timer = new CTimer(TimerCallback, this, Timeout.Infinite, 10000);
            _power = power;
            _enqueueCmd = enqueueAction;

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
            Poll();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            Disposed = true;
        }

        ~PollManager()
        {
            Dispose(false);
        }

        #endregion

        public void Start()
        {
            _timer.Reset(0, 15000);
        }

        void Poll()
        {
            using (var wh = new CEvent())
            {
                _enqueueCmd(new PowerPollCmd());

                if (!_power.PowerIsOn)
                    return;

                _enqueueCmd(new SourcePollCmd());
                _enqueueCmd(new LampPollCmd());
                _enqueueCmd(new MutePollCmd());
            };
        }
    }
}