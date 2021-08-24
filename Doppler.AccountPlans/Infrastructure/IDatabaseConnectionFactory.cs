using System.Data;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface IDatabaseConnectionFactory
    {
        Task<IDbConnection> GetConnection();
    }
}
