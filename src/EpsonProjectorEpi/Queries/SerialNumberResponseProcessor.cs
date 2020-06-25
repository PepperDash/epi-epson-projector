using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using EpsonProjectorEpi.Extensions;

namespace EpsonProjectorEpi.Queries
{
    public class SerialNumberResponseProcessor : BaseResponseProcessor<string>
    {
        private readonly string _response;

        public SerialNumberResponseProcessor(string key, string response)
            : base(key)
        {
            _response = response;
        }

        public override string Handle()
        {
            if (!_response.Contains("SNO"))
                throw new ArgumentException("SNO");

            return _response.TrimResponseForStringValue();
        }
    }
}