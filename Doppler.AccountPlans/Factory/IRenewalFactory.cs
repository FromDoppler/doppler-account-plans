using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.RenewalHandlers;

namespace Doppler.AccountPlans.Factory
{
    public interface IRenewalFactory
    {
        RenewalHandler CreateHandler(int renewalType);
    }
}
