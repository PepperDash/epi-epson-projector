using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PepperDash.Core;
using Crestron.SimplSharp;

namespace EpsonProjectorEpi.Queries
{
    public abstract class BaseResponseProcessor<T> : IKeyed
    {
        readonly string _key;

        #region IKeyed Members

        public string Key
        {
            get { return _key; }
        }

        #endregion

        public BaseResponseProcessor(string key)
        {
            _key = key;
        }

        public abstract T Handle();
    }
}