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
using EpsonProjectorEpi.States.Power;

namespace EpsonProjectorEpi.States
{
    public class LampHoursStateManager : BaseStateManager<int>
    {
        private readonly string _key;

        public LampHoursStateManager(string key, IBasicCommunication coms)
            : base(coms)
        {
            _key = key;
        }

        protected override void ProcessData(string data)
        {
            if (!data.Contains("LAMP"))
                return;

            State = new LampResponseProcessor(Key, data).Handle();
            OnStateUpdated();
        }

        public override string Key
        {
            get { return _key; }
        }
    }
}