using System.Data;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection GetConnection();
    }
}
