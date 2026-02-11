using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Promotion> GetPromotionByCodeAndPlanId(string code, int planId, bool wasApplied)
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

        public async Task<Promotion> GetPromotionByCode(string code)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

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
    [Active] = 1",
                new
                {
                    code
                });

            return promotion;
        }

        public async Task<IList<Promotion>> GetPromotionsByCode(string code)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

            var promotion = await connection.QueryAsync<Promotion>(@"
SELECT
    [IdPromotion],
    [IdUserTypePlan],
    [CreationDate],
    [ExpiringDate] AS ExpirationDate,
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
    [Active] = 1",
                new
                {
                    code,
                });

            return promotion.ToList();
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


        public async Task<TimesApplyedPromocode> GetHowManyTimesApplyedPromocode(string code, string accountName, int planType)
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
    B.DiscountPlanFeePromotion IS NOT NULL AND
    ((@planType = 1 AND B.IdBillingCreditType IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17)) OR --Email Marketing
    (@planType = 2 AND B.IdBillingCreditType IN (28, 29, 30, 31, 32)) OR --Chat
    (@planType = 3 AND B.IdBillingCreditType IN (23, 24, 25, 26, 27)) OR --Landing
    (@planType = 4 AND B.IdBillingCreditType IN (34, 35, 36, 37, 38)) OR --OnSite
    (@planType = 5 AND B.IdBillingCreditType IN (40, 41, 42, 43, 44))) --Push Notification",
                new
                {
                    code,
                    email = accountName,
                    planType
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
    (B.DiscountPlanFeePromotion IS NOT NULL OR B.ExtraCreditsPromotion IS NOT NULL)",
                new
                {
                    email = accountName
                });

            return promotion;
        }

        public async Task<Promotion> GetAddOnPromotionByCodeAndAddOnType(string code, int addOnTypeId, bool wasApplied)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

            var promotion = await connection.QueryFirstOrDefaultAsync<Promotion>(@"
SELECT
    AP.[IdPromotion],
    AP.[CreationDate],
    [ExpiringDate],
    [TimesUsed],
    [TimesToUse],
    AP.[Code],
    AP.[Active],
    AP.Discount as DiscountPercentage,
    AP.[Duration],
    AP.IdAddOnPlan
FROM [AddOnPromotion] AP  WITH(NOLOCK)
INNER JOIN [Promotions] P WITH(NOLOCK) ON P.IdPromotion = AP.IdPromotion
WHERE
    AP.[Code] = @code AND
    AP.[IdAddOnType] = @addOnTypeId AND
    (AP.[Active] = 1 OR @wasApplied = 1) AND
    ([TimesToUse] is null OR [TimesToUse] > [TimesUsed]) AND
    ([ExpiringDate] is null OR [ExpiringDate] >= @now)",
                new
                {
                    code,
                    addOnTypeId,
                    @now = DateTime.Now,
                    wasApplied
                });

            return promotion;
        }


        public async Task<IList<Promotion>> GetAddOnPromotionsByCode(string code, int planId, bool wasApplied)
        {
            IEnumerable<Promotion> addOnpromotions = [];
            var promotion = await GetPromotionByCodeAndPlanId(code, planId, wasApplied);

            if (promotion != null)
            {
                using var _ = _timeCollector.StartScope();
                using var connection = _connectionFactory.GetConnection();

                addOnpromotions = await connection.QueryAsync<Promotion>(@"
SELECT
    AP.[IdPromotion],
    AP.[CreationDate],
    [ExpiringDate],
    [TimesUsed],
    [TimesToUse],
    AP.[Code],
    AP.[Active],
    AP.Discount as DiscountPercentage,
    AP.[Duration],
    AP.IdAddOnPlan,
    AP.IdAddOnType
FROM [AddOnPromotion] AP  WITH(NOLOCK)
INNER JOIN [Promotions] P WITH(NOLOCK) ON P.IdPromotion = AP.IdPromotion
WHERE
    P.IdPromotion = @idPromotion AND
    (AP.[Active] = 1 OR @wasApplied = 1) AND
    ([TimesToUse] is null OR [TimesToUse] > [TimesUsed]) AND
    ([ExpiringDate] is null OR [ExpiringDate] >= @now)",
                    new
                    {
                        idPromotion = promotion.IdPromotion,
                        @now = DateTime.Now,
                        wasApplied
                    });
            }

            return [.. addOnpromotions];
        }

        public async Task<Promotion> GetAddOnPromotionByIdAndAddOnType(int promotionId, int addOnTypeId, bool wasApplied)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

            var promotion = await connection.QueryFirstOrDefaultAsync<Promotion>(@"
SELECT
    AP.[IdPromotion],
    AP.[CreationDate],
    [ExpiringDate],
    [TimesUsed],
    [TimesToUse],
    AP.[Code],
    AP.[Active],
    AP.Discount as DiscountPercentage,
    AP.[Duration],
    AP.[IdAddOnPlan]
FROM [AddOnPromotion] AP  WITH(NOLOCK)
INNER JOIN [Promotions] P WITH(NOLOCK) ON P.IdPromotion = AP.IdPromotion
WHERE
    AP.[IdPromotion] = @promotionId AND
    AP.[IdAddOnType] = @addOnTypeId AND
    (AP.[Active] = 1 OR @wasApplied = 1) AND
    ([TimesToUse] is null OR [TimesToUse] > [TimesUsed]) AND
    ([ExpiringDate] is null OR [ExpiringDate] >= @now)",
                new
                {
                    promotionId,
                    addOnTypeId,
                    wasApplied,
                    @now = DateTime.Now
                });

            return promotion;
        }
    }
}
