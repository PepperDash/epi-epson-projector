using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace EpsonProjectorEpi.Commands
{
    public class PowerPollCmd : IEpsonCmd 
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "PWR?"; }
        }

        #endregion
    }

    public class SourcePollCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "SOURCE?"; }
        }

        #endregion
    }

    public class LampPollCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "LAMP?"; }
        }

        #endregion
    }

    public class MutePollCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "MUTE?"; }
        }

        #endregion
    }

    public class SerialNumberPollCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "SNO?"; }
        }

        #endregion
    }
}