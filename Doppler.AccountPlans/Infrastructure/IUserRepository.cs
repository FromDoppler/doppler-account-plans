using Doppler.AccountPlans.Model;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface IUserRepository
    {
        Task<BillingInformation> GetBillingInformation(string email);
    }
}
