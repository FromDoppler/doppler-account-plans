using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Mappers
{
    public class AddOnMapper : IAddOnMapper
    {
        private readonly IAccountPlansRepository accountPlansRepository;

        public AddOnMapper(IAccountPlansRepository accountPlansRepository)
        {
            this.accountPlansRepository = accountPlansRepository;

        }
        public async Task<AddOnPlan> GetAddOnPlan(int addOnType, int planId)
        {
            return await accountPlansRepository.GetAddOnPlanByAddOnTypeAndAddOnPlanId(addOnType, planId);
        }

        public async Task<IEnumerable<BasePlanInformation>> GetAddOnPlans(int addOnType, bool onlyCustomPlans)
        {
            return onlyCustomPlans
                ? await accountPlansRepository.GetCustomAddOnPlans(addOnType)
                : await accountPlansRepository.GetAddOnPlans(addOnType);
        }

        public async Task<AddOnPlan> GetFreePlan(int addOnType)
        {
            return await accountPlansRepository.GetFreeAddOnPlan(addOnType);
        }
    }
}
