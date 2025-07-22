using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using System.Collections.Generic;
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

        public async Task<IEnumerable<BasePlanInformation>> GetAddOnPlans(bool onlyCustomPlans = false)
        {
            return onlyCustomPlans
                ? await accountPlansRepository.GetCustomOnSitePlans()
                : await accountPlansRepository.GetOnSitePlans();
        }

        public async Task<AddOnPlan> GetFreePlan()
        {
            return await accountPlansRepository.GetFreeOnSitePlan();
        }
    }
}
