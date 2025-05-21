using System.Threading.Tasks;
using Doppler.AccountPlans.Model;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface IPromotionRepository
    {
        Task<Promotion> GetPromotionByCode(string code, int planId, bool wasApplied);
        Task<TimesApplyedPromocode> GetHowManyTimesApplyedPromocode(string code, string accountName);
        Task<Promotion> GetCurrentPromotionByAccountName(string accountName);
    }
}
