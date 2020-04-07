using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace EpsonProjectorEpi.Commands
{
    public class PollManager : IDisposable, IPollManager
    {
        public bool Disposed { get; private set; }

        readonly CTimer _timer;

        public PollManager(IBasicCommunication coms)
        {
            _timer = new CTimer(TimerCallback, coms, Timeout.Infinite, 10000);
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
                wh.Wait(25);

                new CmdHandler(coms, new SourcePollCmd()).Handle();
                wh.Wait(25);

                new CmdHandler(coms, new LampPollCmd()).Handle();
                wh.Wait(25);

                new CmdHandler(coms, new MutePollCmd()).Handle();
                wh.Wait(25);
            };
        }
    }
}