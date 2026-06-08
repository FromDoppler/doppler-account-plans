using Dapper;
using Doppler.AccountPlans.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public class UserRepository(IDatabaseConnectionFactory connectionFactory) : IUserRepository
    {
        private readonly IDatabaseConnectionFactory connectionFactory = connectionFactory;

        public async Task<BillingInformation> GetBillingInformation(string email)
        {
            using var connection = connectionFactory.GetConnection();

            var results = await connection.QueryAsync<BillingInformation>(@"
SELECT
    U.BillingFirstName AS Firstname,
    U.BillingLastName AS Lastname,
    U.BillingAddress AS Address,
    U.BillingCity AS City,
    isnull(S.StateCode, '') AS Province,
    isnull(CO.Code, '') AS Country,
    U.BillingZip AS ZipCode,
    U.BillingPhone AS Phone,
    U.PaymentMethod AS PaymentMethod
FROM
    [User] U
    LEFT JOIN [State] S ON U.IdBillingState = S.IdState
    LEFT JOIN [Country] CO ON S.IdCountry = CO.IdCountry
WHERE
    U.Email = @email",
                new { email });

            return results.FirstOrDefault();
        }
    }
}
