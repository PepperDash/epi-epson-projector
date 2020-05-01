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
    public abstract class BaseState<T> : IKeyed
    {
        public abstract string Key { get; }
        public bool Disposed { get; private set; }

        protected T _currentState;

        private EpsonProjector _proj;
        public EpsonProjector Proj
        {
            get
            {
                return _proj;
            }
        }

        protected BaseState(EpsonProjector proj)
        {
            _proj = proj;
        }

        protected abstract void UpdateState(T state);

        public T Current { get { return _currentState; } }
    }
}