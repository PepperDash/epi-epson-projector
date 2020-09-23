﻿using System;

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

        public static CustomCmd Empty
        {
            get { return new CustomCmd(String.Empty); }
        }
    }
}