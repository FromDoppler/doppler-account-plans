using Doppler.AccountPlans.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface IAccountPlansRepository
    {
        Task<IEnumerable<PlanRenewalInformation>> GetPlanRenewalInformation(int planId, string paymentMethod);
    }
}
