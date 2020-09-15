using System;
using PepperDash.Core;

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

        public static string TrimResponseForStringValue(string str)
        {
            if (!str.Contains("=")) return str;

            var index = str.IndexOf("=") + 1;
            return str.Remove(0, index).TrimEnd('\x0D');
        }

        public static int TrimResponseForIntValue(string str)
        {
            if (!str.Contains("=")) return 0;

            var index = str.IndexOf("=") + 1;
            return Convert.ToInt32(str.Remove(0, index));
        }
    }
}