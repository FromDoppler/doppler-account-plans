using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Mappers
{
    public class PushNotificationMapper : IAddOnMapper
    {
        private readonly IAccountPlansRepository accountPlansRepository;

        public PushNotificationMapper(IAccountPlansRepository accountPlansRepository)
        {
            this.accountPlansRepository = accountPlansRepository;
        }

        public async Task<AddOnPlan> GetAddOnPlan(int planId)
        {
            return await accountPlansRepository.GetPushNotificationPlanById(planId);
        }

        public async Task<IEnumerable<BasePlanInformation>> GetAddOnPlans(bool onlyCustomPlans = false)
        {
            return onlyCustomPlans
                ? await accountPlansRepository.GetCustomPushNotificationPlans()
                : await accountPlansRepository.GetPushNotificationPlans();
        }

        public Task<AddOnPlan> GetFreePlan()
        {
            return accountPlansRepository.GetFreePushNotificationPlan();
        }
    }
}
