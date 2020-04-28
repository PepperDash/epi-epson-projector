using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace EpsonProjectorEpi.States
{
    public interface IStateHasPoll
    {
        void Poll();
    }
}