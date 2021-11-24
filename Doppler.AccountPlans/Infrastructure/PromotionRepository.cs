using System;
using System.Threading.Tasks;
using Dapper;
using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Model;

namespace Doppler.AccountPlans.Infrastructure
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public PromotionRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Promotion> GetPromotionByCode(string code, int planId)
        {
            using var connection = await _connectionFactory.GetConnection();

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
    [Promotions]
WHERE
    [Code] = @code AND
    [Active] = 1 AND
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
                    @now = DateTime.Now
                });

            return promotion;
        }

        private async Task<int> GetUserTypeByPlan(int planId)
        {
            using var connection = await _connectionFactory.GetConnection();

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
    }
}
