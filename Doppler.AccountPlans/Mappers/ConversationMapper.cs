using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Mappers
{
    public class ConversationMapper(IAccountPlansRepository accountPlansRepository) : IAddOnMapper
    {
        private readonly IAccountPlansRepository accountPlansRepository = accountPlansRepository;

        public async Task<AddOnPlan> GetAddOnPlan(int planId)
        {
            return await accountPlansRepository.GetConversationPlanById(planId);
        }

        public async Task<IEnumerable<BasePlanInformation>> GetAddOnPlans(bool onlyCustomPlans = false)
        {
            return onlyCustomPlans
                ? await accountPlansRepository.GetCustomConversationPlans()
                : await accountPlansRepository.GetConversationPlans();
        }

        public async Task<AddOnPlan> GetFreePlan()
        {
            return await accountPlansRepository.GetFreeConversationPlan();
        }
    }
}
