using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Mappers
{
    public class OnSiteMapper : IAddOnMapper
    {
        private readonly IAccountPlansRepository accountPlansRepository;

        public OnSiteMapper(IAccountPlansRepository accountPlansRepository)
        {
            this.accountPlansRepository = accountPlansRepository;
        }

        public async Task<AddOnPlan> GetAddOnPlan(int planId)
        {
            return await accountPlansRepository.GetOnSitePlanById(planId);
        }
    }
}
