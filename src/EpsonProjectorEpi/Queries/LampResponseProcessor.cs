using System;

namespace EpsonProjectorEpi.Queries
{
    public class LampResponseProcessor : BaseResponseProcessor<int>
    {
        readonly string _response;

        public LampResponseProcessor(string key, string response)
            : base(key)
        {
            _response = response;
        }

        public override int Handle()
        {
            if (!_response.Contains("LAMP"))
                throw new ArgumentException("LAMP");

            return TrimResponseForIntValue(_response);
        }
    }
}