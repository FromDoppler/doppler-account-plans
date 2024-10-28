using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface IAccountPlansRepository
    {
        Task<IEnumerable<PlanDiscountInformation>> GetPlanDiscountInformation(int planId, string paymentMethod);
        Task<PlanInformation> GetPlanInformation(int planId);
        Task<UserPlanInformation> GetCurrentPlanInformation(string accountName);
        Task<PlanDiscountInformation> GetDiscountInformation(int discountId);
        Task<UserPlanInformation> GetFirstUpgrade(string accountName);
        Task<int> GetAvailableCredit(string accountName);

        Task<PlanInformation> GetChatPlanInformation(int chatPlanId);
        Task<UserPlanInformation> GetCurrentPlanInformationWithAdditionalServices(string accountName);
        Task<BasePlanInformation> GetPlanInformation(PlanTypeEnum planType, int planId);
        Task<IEnumerable<ConversationPlanInformation>> GetConversationPlans();
        Task<IEnumerable<LandingPlanInformation>> GetLandingPlans();
        Task<UserPlanInformation> GetLastLandingPlanBillingInformation(string accountName);

        Task<UserPlanInformation> GetFirstLandingUpgrade(string accountName);

        Task<IEnumerable<ConversationPlanInformation>> GetCustomConversationPlans();

        Task<DateTime?> GetFirstUpgradeDate(string accountName);
        Task<IEnumerable<OnSitePlanInformation>> GetOnSitePlans();
    }
}
