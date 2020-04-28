using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi.States
{
    public abstract class BaseState<T> : IKeyed, IDisposable
    {
        public abstract string Key { get; }
        public bool Disposed { get; private set; }

        protected T _currentState;
        private readonly IStateManager<T> _stateManager;
        public IStateManager<T> StateManager { get { return _stateManager; } }

        private EpsonProjector _proj;
        public EpsonProjector Proj
        {
            get
            {
                return _proj;
            }
        }

        protected BaseState(EpsonProjector proj, IStateManager<T> stateManager)
        {
            _proj = proj;
            _stateManager = stateManager;
            _stateManager.StateUpdated += ManageStateUpdate;
        }

        private void ManageStateUpdate(object sender, EventArgs args)
        {
            var manager = sender as IStateManager<T>;
            UpdateState(manager.State);
        }

        protected abstract void UpdateState(T state);

        public T Current { get { return _currentState; } }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                _stateManager.StateUpdated -= ManageStateUpdate;
            }

            Disposed = true;
        }

        ~BaseState()
        {
            Dispose(false);
        }

        #endregion
    }
}