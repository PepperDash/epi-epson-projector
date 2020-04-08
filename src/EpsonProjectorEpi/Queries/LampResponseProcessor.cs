using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using EpsonProjectorEpi.Extensions;

namespace EpsonProjectorEpi.Queries
{
    public class LampResponseProcessor : BaseResponseProcessor
    {
        readonly string _response;

        public LampResponseProcessor(string key, string response)
            : base(key)
        {
            _response = response;
        }

        public int Handle()
        {
            if (!_response.Contains("LAMP"))
                throw new ArgumentException("LAMP");

            return _response.TrimResponseForIntValue();
        }
    }
}