using PepperDash.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Queries;

namespace EpsonProjectorEpi.States
{
    public class ResponseStateManager<T, TResponse> :
        BaseStateManager<TResponse> where T : ResponseEnumeration<T, TResponse>
    {
        private readonly string _key;

        public ResponseStateManager(string key, CommunicationGather gather)
            : base(gather)
        {
            _key = key;
            ResponseEnumeration<T, TResponse>.GetAll();
        }

        protected override void ProcessData(string data)
        {
            if (!data.Contains(ResponseEnumeration<T, TResponse>.SearchString))
                return;

            var result = new ResponseProcessor<T, TResponse>(Key, data).Handle();

            if (result == null)
                return;

            OnStateUpdated(result.ResponseState);
        }

        public override string Key
        {
            get { return _key; }
        }
    }
}