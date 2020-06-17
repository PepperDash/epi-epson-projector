using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using PepperDash.Core;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi.Queries
{
    public class ResponseProcessor<T, TResponse> : BaseResponseProcessor<T> 
        where T : ResponseEnumeration<T, TResponse>
    {
        readonly string _response;

        public ResponseProcessor(string key, string response)
            : base(key)
        {
            _response = response;
        }

        public override T Handle()
        {
            Debug.Console(1, this, "Checking response: '{0}'", _response);
            return ResponseEnumeration<T, TResponse>.GetAll().FirstOrDefault(x => Regex.IsMatch(_response, x.ResponseString));
        }
    }
}