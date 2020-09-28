using PepperDash.Core;
using EpsonProjectorEpi.Queries;

namespace EpsonProjectorEpi.States
{
    public class SerialNumberStateManager : BaseStateManager<string>
    {
        private readonly string _key;

        public SerialNumberStateManager(string key, CommunicationGather gather)
            : base(gather)
        {
            _key = key;
        }

        protected override void ProcessData(string data)
        {
            if (!data.Contains("SNO"))
                return;

            var result = new SerialNumberResponseProcessor(Key, data).Handle();
            OnStateUpdated(result);
        }

        public override string Key
        {
            get {return _key; }
        }
    }
}