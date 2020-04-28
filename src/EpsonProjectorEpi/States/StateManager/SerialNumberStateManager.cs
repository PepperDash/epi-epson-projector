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
    public class SerialNumberStateManager : BaseStateManager<string>
    {
        private readonly string _key;

        public SerialNumberStateManager(string key, IBasicCommunication coms)
            : base(coms)
        {
            _key = key;
        }

        protected override void ProcessData(string data)
        {
            if (!data.Contains("SNO"))
                return;

            State = new SerialNumberResponseProcessor(Key, data).Handle();
            OnStateUpdated();
        }

        public override string Key
        {
            get {return _key; }
        }
    }
}