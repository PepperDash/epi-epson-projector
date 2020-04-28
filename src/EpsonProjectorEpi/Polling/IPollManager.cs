using System;

namespace EpsonProjectorEpi.Polling
{
    public interface IPollManager : IDisposable
    {
        bool Disposed { get; }
        void Start();
    }
}
