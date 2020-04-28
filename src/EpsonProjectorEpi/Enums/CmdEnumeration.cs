using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using EpsonProjectorEpi.Commands;

namespace EpsonProjectorEpi.Enums
{
    public abstract class CmdEnumeration<T> : Enumeration<T> where T : CmdEnumeration<T>
    {
        public IEpsonCmd Cmd { get; private set; }
        public string Response { get; private set; }
        public static string SearchString { get; protected set; }
        public static CmdEnumeration<T> Default { get; protected set; }
        public static CmdEnumeration<T> Unknown { get; protected set; }  

        protected CmdEnumeration(int value, string name, IEpsonCmd cmd, string response)
            : base (value, name)
        {
            Cmd = cmd;
            Response = response;
        }
    }
}