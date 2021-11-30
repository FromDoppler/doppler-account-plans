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
    DP.[MonthPlan],
    DP.[ApplyPromo]
FROM
    [DiscountXPlan] DP INNER JOIN [PaymentMethods] PM ON DP.IdPaymentMethod = PM.IdPaymentMethod
WHERE
    DP.[IdUserTypePlan] = @planId AND PM.PaymentMethodName = @paymentMethod AND DP.Active = 1
ORDER BY
    DP.[MonthPlan]",
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

        public async Task<PlanInformation> GetCurrentPlanInformation(string accountName)
        {
            using var connection = await _connectionFactory.GetConnection();

            var currentPlan = await connection.QueryFirstOrDefaultAsync<PlanInformation>(@"
SELECT
    B.[PlanFee] AS Fee,
    B.[CurrentMonthPlan],
    UTP.[IdUserType]
FROM
    [BillingCredits] B
INNER JOIN [UserTypesPlans] UTP ON UTP.IdUserTypePlan = B.IdUserTypePlan
WHERE
    b.IdUser = (SELECT IdUser FROM [User] WHERE Email = @email) ORDER BY b.[Date] DESC;",
                new
                {
                    @email = accountName
                });

            return currentPlan;
        }

        public async Task<PlanDiscountInformation> GetDiscountInformation(int discountId)
        {
            using var connection = await _connectionFactory.GetConnection();

            var discountPlan = await connection.QueryFirstOrDefaultAsync<PlanDiscountInformation>(@"
SELECT
    d.[MonthPlan],
    d.[DiscountPlanFee],
    d.[ApplyPromo]
FROM
    DiscountXPlan d
WHERE
    d.[IdDiscountPlan] = @discountId;",
                new
                {
                    discountId
                });

            return discountPlan;
        }
    }
}
