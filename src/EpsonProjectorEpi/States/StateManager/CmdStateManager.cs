using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Queries;
using EpsonProjectorEpi.States.Power;

namespace EpsonProjectorEpi.States
{
    public class CmdStateManager<T> : BaseStateManager<T> where T : CmdEnumeration<T>
    {
        private readonly string _key;

        public CmdStateManager(string key, IBasicCommunication coms)
            : base(coms)
        {
            _key = key;
        }

        protected override void ProcessData(string data)
        {
            var searchString = CmdEnumeration<T>.SearchString;
            if (!data.Contains(searchString))
                return;

            var result = new CmdResponseProcessor<T>(Key, data).Handle();

            State = result;
            OnStateUpdated();
        }

        public override string Key
        {
            get { return _key; }
        }
    }
}