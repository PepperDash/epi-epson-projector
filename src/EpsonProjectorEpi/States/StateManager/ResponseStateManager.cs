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
    public class ResponseStateManager<T, TResponse> :
        BaseStateManager<TResponse> where T : ResponseEnumeration<T, TResponse>
    {
        private readonly string _key;

        public ResponseStateManager(string key, IBasicCommunication coms)
            : base(coms)
        {
            _key = key;
            ResponseEnumeration<T, TResponse>.GetAll();
        }

        protected override void ProcessData(string data)
        {
            if (!data.Contains(ResponseEnumeration<T, TResponse>.SearchString))
                return;

            var result = new ResponseProcessor<T, TResponse>(Key, data).Handle();

            if (result == null)
                return;

            OnStateUpdated(result.ResponseState);
        }

        public override string Key
        {
            get { return _key; }
        }
    }
}