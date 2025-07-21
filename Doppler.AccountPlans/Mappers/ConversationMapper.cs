using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Mappers
{
    public class ConversationMapper : IAddOnMapper
    {
        private readonly IAccountPlansRepository accountPlansRepository;

        public ConversationMapper(IAccountPlansRepository accountPlansRepository)
        {
            this.accountPlansRepository = accountPlansRepository;
        }

        public Task<AddOnPlan> GetAddOnPlan(int planId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BasePlanInformation>> GetAddOnPlans(bool onlyCustomPlans = false)
        {
            throw new NotImplementedException();
        }

        public async Task<AddOnPlan> GetFreePlan()
        {
            return await accountPlansRepository.GetFreeConversationPlan();
        }
    }
}
