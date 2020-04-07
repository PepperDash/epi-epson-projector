using System;

namespace EpsonProjectorEpi.Commands
{
    public interface IPollManager
    {
        void Dispose();
        bool Disposed { get; }
        void Start();
    }
}
