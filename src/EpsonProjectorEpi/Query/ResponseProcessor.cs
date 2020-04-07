using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using EpsonProjectorEpi.Enums;

namespace EpsonProjectorEpi.Queries
{
    public class ResponseProcessor<T> : BaseResponseProcessor where T : CmdEnumeration<T>
    {
        readonly string _response;

        public ResponseProcessor(string key, string response)
            : base(key)
        {
            _response = response;
        }

        public T Handle()
        {
            if (String.IsNullOrEmpty(_response) || !_response.Contains(CmdEnumeration<T>.SearchString))
                return null;

            return CmdEnumeration<T>.GetAll().FirstOrDefault(x => _response.Equals(x.Response));
        }
    }
}