using System;
using System.Threading.Tasks;
using Dapper;
using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.TimeCollector;

namespace Doppler.AccountPlans.Infrastructure
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        private readonly ITimeCollector _timeCollector;

        public PromotionRepository(IDatabaseConnectionFactory connectionFactory, ITimeCollector timeCollector)
        {
            _connectionFactory = connectionFactory;
            _timeCollector = timeCollector;
        }

        public async Task<Promotion> GetPromotionByCode(string code, int planId, bool wasApplied)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

            var userType = await GetUserTypeByPlan(planId);

            var promotion = await connection.QueryFirstOrDefaultAsync<Promotion>(@"
SELECT
    [IdPromotion],
    [IdUserTypePlan],
    [CreationDate],
    [ExpiringDate],
    [TimesUsed],
    [TimesToUse],
    [Code],
    [ExtraCredits],
    [Active],
    [DiscountPlanFee] as DiscountPercentage,
    [AllPlans],
    [AllSubscriberPlans],
    [AllPrepaidPlans],
    [AllMonthlyPlans],
    [Duration]
FROM
    [Promotions]  WITH(NOLOCK)
WHERE
    [Code] = @code AND
    ([Active] = 1 OR @wasApplied = 1) AND
    ([TimesToUse] is null OR [TimesToUse] > [TimesUsed]) AND
    ([ExpiringDate] is null OR [ExpiringDate] >= @now) AND
    ([IdUserTypePlan] = @planId OR
    [AllPlans] = 1 OR
    (@userType = @individual AND [AllPrepaidPlans] = 1) OR
    (@userType = @subscribers AND [AllSubscriberPlans] = 1) OR
    (@userType = @monthly AND [AllMonthlyPlans] = 1))",
                new
                {
                    code,
                    planId,
                    userType,
                    @individual = (int)UserTypesEnum.Individual,
                    @subscribers = (int)UserTypesEnum.Subscribers,
                    @monthly = (int)UserTypesEnum.Monthly,
                    @now = DateTime.Now,
                    wasApplied
                });

            return promotion;
        }

        private async Task<int> GetUserTypeByPlan(int planId)
        {
            using var connection = _connectionFactory.GetConnection();

            var userPlanTypeId = await connection.QueryFirstOrDefaultAsync<int>(@"
SELECT
    [IdUserType]
FROM
    [UserTypesPlans]
WHERE
    [IdUserTypePlan] = @planId", new
            {
                planId
            });

            return userPlanTypeId;
        }


        public async Task<TimesApplyedPromocode> GetHowManyTimesApplyedPromocode(string code, string accountName)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var times = await connection.QueryFirstOrDefaultAsync<TimesApplyedPromocode>(@"
SELECT
    MAX(MONTH(B.Date)) AS LastMonthApplied,
    MAX(YEAR(B.Date)) AS LastYearApplied,
    COUNT(DISTINCT MONTH(B.Date)) AS CountApplied
FROM
    [BillingCredits] B  WITH(NOLOCK)
INNER JOIN [User] U  WITH(NOLOCK) ON U.IdUser = B.IdUser
INNER JOIN [Promotions] P  WITH(NOLOCK) ON  P.IdPromotion = B.IdPromotion
WHERE
    U.Email = @email AND
    U.IdCurrentBillingCredit IS NOT NULL AND
    P.Code = @code AND
    B.DiscountPlanFeePromotion IS NOT NULL",
                new
                {
                    code,
                    email = accountName
                });

            return times;
        }

        public async Task<Promotion> GetCurrentPromotionByAccountName(string accountName)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

            var promotion = await connection.QueryFirstOrDefaultAsync<Promotion>(@"
SELECT
    P.[IdPromotion],
    [Code],
    [ExtraCredits],
    [Active],
    [DiscountPlanFee] as DiscountPercentage,
    ISNULL(B.PromotionDuration, P.Duration) AS [Duration]
FROM [User] U  WITH(NOLOCK)
INNER JOIN [BillingCredits] B  WITH(NOLOCK) ON B.IdBillingCredit = U.IdCurrentBillingCredit
INNER JOIN [Promotions] P  WITH(NOLOCK) ON  P.IdPromotion = B.IdPromotion
WHERE
    U.Email = @email AND
    B.DiscountPlanFeePromotion IS NOT NULL",
                new
                {
                    email = accountName
                });

            return promotion;
        }
    }
}
