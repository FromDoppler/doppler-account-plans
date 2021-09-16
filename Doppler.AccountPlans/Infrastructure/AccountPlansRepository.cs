using Dapper;
using Doppler.AccountPlans.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public class AccountPlansRepository : IAccountPlansRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        public AccountPlansRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<PlanDiscountInformation>> GetPlanDiscountInformation(int planId, string paymentMethod)
        {
            using var connection = await _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<PlanDiscountInformation>(@"
SELECT
    DP.[IdDiscountPlan],
    DP.[DiscountPlanFee],
    DP.[MonthPlan]
FROM
    [DiscountXPlan] DP INNER JOIN [PaymentMethods] PM ON DP.IdPaymentMethod = PM.IdPaymentMethod
WHERE
    DP.[IdUserTypePlan] = @planId AND PM.PaymentMethodName = @paymentMethod",
    new { planId, paymentMethod });

            return result;
        }

        public async Task<PlanInformation> GetPlanInformation(int planId)
        {
            using var connection = await _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<PlanInformation>(@"
SELECT
    UTP.[IdUserType],
    UTP.[EmailQty],
    UTP.[Fee],
    UTP.[SubscribersQty],
    UTP.[PlanType] AS Type
FROM
    [UserTypesPlans] UTP
WHERE
    UTP.[IdUserTypePlan] = @planId",
    new { planId });

            return result.FirstOrDefault();
        }
    }
}
