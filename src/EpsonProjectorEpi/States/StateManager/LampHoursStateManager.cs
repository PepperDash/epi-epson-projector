﻿using System;
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
    public class LampHoursStateManager : BaseStateManager<int>
    {
        private readonly string _key;

        public LampHoursStateManager(string key, CommunicationGather gather)
            : base(gather)
        {
            _key = key;
        }

        protected override void ProcessData(string data)
        {
            if (!data.Contains("LAMP"))
                return;

            var result = new LampResponseProcessor(Key, data).Handle();
            if (result == 0)
                return;

            OnStateUpdated(result);
        }

        public override string Key
        {
            get { return _key; }
        }
    }
}