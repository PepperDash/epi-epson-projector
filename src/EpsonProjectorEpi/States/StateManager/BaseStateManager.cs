﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Queries;

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

        public T State { get; private set; }

        public event EventHandler<StateUpdatedEventArgs<T>> StateUpdated;

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

        protected virtual void OnStateUpdated(T state)
        {
            if (state.Equals(State))
                return;

            State = state;

            var handler = StateUpdated;
            if (handler == null) return;

            handler.Invoke(this, new StateUpdatedEventArgs<T> { CurrentState = state });
        }
    }

    public class StateUpdatedEventArgs<T> : EventArgs
    {
        public T CurrentState { get; set; }
    }
}