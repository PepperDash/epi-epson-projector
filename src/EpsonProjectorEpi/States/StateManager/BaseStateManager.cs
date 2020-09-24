using System;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace EpsonProjectorEpi.States
{
    public abstract class BaseStateManager<T> : IStateManager<T>
    {
        public abstract string Key { get; }
        readonly CommunicationGather _gather;

        public BaseStateManager(CommunicationGather gather)
        {
            _gather = gather;
            _gather.LineReceived += HandleLineReceived;
        }

        protected virtual void HandleLineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            if (String.IsNullOrEmpty(e.Text)) 
                return;

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
            State = state;
            Debug.Console(1, this, "Received state update: '{0}'", state.ToString());

            var handler = StateUpdated;
            if (handler == null) 
                return;

            handler.Invoke(this, new StateUpdatedEventArgs<T> { CurrentState = state });
        }
    }

    public class StateUpdatedEventArgs<T> : EventArgs
    {
        public T CurrentState { get; set; }
    }
}