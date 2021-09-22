using Dapper;
using Doppler.AccountPlans.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.AccountPlans.Factory;

namespace Doppler.AccountPlans.Infrastructure
{
    public class AccountPlansRepository : IAccountPlansRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IRenewalFactory _renewalFactory;

        public AccountPlansRepository(IDatabaseConnectionFactory connectionFactory, IRenewalFactory renewalFactory)
        {
            _connectionFactory = connectionFactory;
            _renewalFactory = renewalFactory;
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

        public async Task<PlanAmountDetails> GetPlanAmountDetails(int newPlanId, string accountName, int discountId)
        {
            using var connection = await _connectionFactory.GetConnection();

            var currentDiscountPlan = await connection.QueryFirstOrDefaultAsync<PlanDiscountInformation>(@"
SELECT
    d.[MonthPlan],
    d.[DiscountPlanFee]
FROM
    DiscountXPlan d
WHERE
    d.[IdDiscountPlan] = @discountId;",
                new
                {
                    discountId
                });

            var renewalHandler = _renewalFactory.CreateHandler(currentDiscountPlan.MonthPlan);

            if (renewalHandler == null)
                return null;

            var newPlan = await GetPlanInformation(newPlanId);

            var currentPlan = await connection.QueryFirstOrDefaultAsync<PlanInformation>(@"
SELECT
    b.[PlanFee] AS Fee,
    b.[CurrentMonthPlan]
FROM
    [BillingCredits] b
WHERE
    b.IdUser = (SELECT IdUser FROM [User] WHERE Email = @email) ORDER BY b.[Date] DESC;",
                new
                {
                    @email = accountName
                });

            return renewalHandler.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan);
        }
    }
}
