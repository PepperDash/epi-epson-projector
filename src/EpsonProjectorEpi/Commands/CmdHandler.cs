using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace EpsonProjectorEpi.Commands
{
    public class CmdHandler
    {
        readonly IBasicCommunication _coms;
        readonly IEpsonCmd _cmd;

        public CmdHandler(IBasicCommunication coms, IEpsonCmd cmd)
        {
            _coms = coms;
            _cmd = cmd;
        }

        public bool Handle()
        {
            if (_cmd == null || String.IsNullOrEmpty(_cmd.CmdString))
                return false;

            Debug.Console(2, _coms, "Attempting to send string {0}", _cmd.CmdString);

            _coms.SendText(_cmd.CmdString + '\x0D');
            return true;
        }
    }

    public interface IEpsonCmd
    {
        string CmdString { get; }
    }
}