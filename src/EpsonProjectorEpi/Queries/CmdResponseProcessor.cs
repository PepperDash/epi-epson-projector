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
    public class CmdResponseProcessor<T> : BaseResponseProcessor<T> where T : CmdEnumeration<T>
    {
        readonly string _response;

        public CmdResponseProcessor(string key, string response)
            : base(key)
        {
            _response = response;
        }

        public override T Handle()
        {
            return CmdEnumeration<T>.GetAll().FirstOrDefault(x => Regex.IsMatch(_response, x.Response));
        }
    }
}