using Doppler.AccountPlans.Model;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Mappers
{
    public interface IAddOnMapper
    {
        Task<AddOnPlan> GetAddOnPlan(int planId);
    }
}
