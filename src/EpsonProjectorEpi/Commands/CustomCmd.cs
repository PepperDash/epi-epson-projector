using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace EpsonProjectorEpi.Commands
{
    public class CustomCmd : IEpsonCmd
    {
        readonly string _cmd;

        public CustomCmd(string cmd)
        {
            _cmd = cmd;
        }

        #region IEpsonCmd Members

        public string CmdString
        {
            get { return !String.IsNullOrEmpty(_cmd) ? _cmd : String.Empty; }
        }

        #endregion
    }
}