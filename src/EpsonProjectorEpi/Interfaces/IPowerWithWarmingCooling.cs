using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace EpsonProjectorEpi.Interfaces
{
    public interface IPowerWithWarmingCooling : IPower, IWarmingCooling, IKeyed
    {
    }
}