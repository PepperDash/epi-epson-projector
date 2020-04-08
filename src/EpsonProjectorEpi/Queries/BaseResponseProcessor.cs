using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PepperDash.Core;
using Crestron.SimplSharp;

namespace EpsonProjectorEpi.Queries
{
    public class BaseResponseProcessor : IKeyed
    {
        readonly string _key;

        #region IKeyed Members

        public string Key
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public BaseResponseProcessor(string key)
        {
            _key = key;
        }
    }
}