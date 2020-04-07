using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace EpsonProjectorEpi.Commands
{
    public class MuteOnCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "MUTE ON"; }
        }

        #endregion
    }

    public class MuteOffCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "MUTE OFF"; }
        }

        #endregion
    }
}