using Doppler.AccountPlans.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Mappers
{
    public interface IAddOnMapper
    {
        Task<AddOnPlan> GetAddOnPlan(int addOnType, int planId);
        Task<IEnumerable<BasePlanInformation>> GetAddOnPlans(int addOnType, bool onlyCustomPlans);
        Task<AddOnPlan> GetFreePlan(int addOnType);
    }
}
