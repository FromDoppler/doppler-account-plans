using Doppler.AccountPlans.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface IAccountPlansRepository
    {
        Task<IEnumerable<PlanDiscountInformation>> GetPlanDiscountInformation(int planId, string paymentMethod);
        Task<PlanInformation> GetPlanInformation(int planId);
        Task<PlanAmountDetails> GetPlanAmountDetails(int newPlanId, string accountName, int discountId);
    }
}
