using System;
using PepperDash.Core;

namespace EpsonProjectorEpi.States
{
    public interface IStateManager<T> : IDisposable, IKeyed
    {
        T State { get; }
        event EventHandler<StateUpdatedEventArgs<T>> StateUpdated;
    }
}
