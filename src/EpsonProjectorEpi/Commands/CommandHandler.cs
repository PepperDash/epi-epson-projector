using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace EpsonProjectorEpi.Commands
{
    public class CommandHandler
    {
        readonly IBasicCommunication _coms;
        readonly IEpsonCmd _cmd;

        public CommandHandler(IBasicCommunication coms, IEpsonCmd cmd)
        {
            _coms = coms;
            _cmd = cmd;
        }

        public bool Handle()
        {
            if (_cmd == null || String.IsNullOrEmpty(_cmd.CmdString))
                return false;

            var cmd = new StringBuilder(_cmd.CmdString);
            cmd.Append('\x0D');

            _coms.SendText(cmd.ToString());
            return true;
        }
    }

    public interface IEpsonCmd
    {
        string CmdString { get; }
    }
}