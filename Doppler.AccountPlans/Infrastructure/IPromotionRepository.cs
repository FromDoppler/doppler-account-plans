using System.Collections.Generic;
using System.Threading.Tasks;
using Doppler.AccountPlans.Model;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface IPromotionRepository
    {
        Task<Promotion> GetPromotionByCode(string code, int planId, bool wasApplied);
        Task<TimesApplyedPromocode> GetHowManyTimesApplyedPromocode(string code, string accountName, int planType);
        Task<Promotion> GetCurrentPromotionByAccountName(string accountName);
        Task<Promotion> GetAddOnPromotionByCodeAndAddOnType(string code, int addOnTypeId, bool wasApplied);
        Task<IList<Promotion>> GetAddOnPromotionsByCode(string code, int planId, bool wasApplied);
    }
}
