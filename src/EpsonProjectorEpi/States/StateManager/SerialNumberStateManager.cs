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

            var result = new SerialNumberResponseProcessor(Key, data).Handle();
            OnStateUpdated(result);
        }

        public override string Key
        {
            get {return _key; }
        }
    }
}