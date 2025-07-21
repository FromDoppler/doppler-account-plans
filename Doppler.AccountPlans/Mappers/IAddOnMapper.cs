using Doppler.AccountPlans.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Mappers
{
    public interface IAddOnMapper
    {
        Task<AddOnPlan> GetAddOnPlan(int planId);
        Task<IEnumerable<BasePlanInformation>> GetAddOnPlans(bool onlyCustomPlans);
        Task<AddOnPlan> GetFreePlan();
    }
}
