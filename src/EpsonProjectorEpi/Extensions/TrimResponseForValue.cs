using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace EpsonProjectorEpi.Extensions
{
    public static class StringExtensions
    {
        public static string TrimResponseForStringValue(this string str)
        {
            if (!str.Contains("=")) return str;

            var index = str.IndexOf("=") + 1;
            return str.Remove(0, index).TrimEnd('\x0D');
        }

        public static int TrimResponseForIntValue(this string str)
        {
            if (!str.Contains("=")) return 0;

            var index = str.IndexOf("=") + 1;
            return Convert.ToInt32(str.Remove(0, index));
        }
    }
}