using System.Threading.Tasks;
using Doppler.AccountPlans.Model;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface IPromotionRepository
    {
        Task<Promotion> GetPromotionByCode(string code, int planId);
    }
}
