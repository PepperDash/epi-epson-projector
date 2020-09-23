using System;

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

            return TrimResponseForStringValue(_response);
        }
    }
}