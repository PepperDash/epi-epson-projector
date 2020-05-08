using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Queries;
using EpsonProjectorEpi.States.Power;

namespace EpsonProjectorEpi.States
{
    public abstract class BaseStateManager<T> : IStateManager<T>
    {
        public abstract string Key { get; }
        readonly IBasicCommunication _coms;
        readonly CommunicationGather _gather;

        public BaseStateManager(IBasicCommunication coms)
        {
            _coms = coms;
            _gather = new CommunicationGather(coms, "\x0D:");
            _gather.LineReceived += HandleLineReceived;
        }

        protected virtual void HandleLineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            if (String.IsNullOrEmpty(e.Text)) return;
            ProcessData(e.Text);
        }

        protected abstract void ProcessData(string data);

        #region IStateManager<T> Members

        public T State { get; protected set; }

        public event EventHandler StateUpdated;

        #endregion

        bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing) 
            {
                _gather.LineReceived -= HandleLineReceived;
                _gather.Stop();
            }

            _disposed = true;
        }

        ~BaseStateManager()
        {    
            Dispose(false);
        }

        protected virtual void OnStateUpdated()
        {
            var handler = StateUpdated;
            if (handler == null) return;

            handler.Invoke(this, EventArgs.Empty);
        }
    }
}