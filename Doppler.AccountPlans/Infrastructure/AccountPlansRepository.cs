using Dapper;
using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Model;
using Microsoft.AspNetCore.Connections;
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
            using var connection = _connectionFactory.GetConnection();
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
            using var connection = _connectionFactory.GetConnection();
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

        public async Task<UserPlanInformation> GetCurrentPlanInformation(string accountName)
        {
            using var connection = _connectionFactory.GetConnection();

            var currentPlan = await connection.QueryFirstOrDefaultAsync<UserPlanInformation>(@"
SELECT
    B.[PlanFee] AS Fee,
    B.[CurrentMonthPlan],
    UTP.[IdUserType],
    B.[DiscountPlanFeeAdmin],
    B.[DiscountPlanFeePromotion],
    P.Code AS PromotionCode,
    B.IdUserTypePlan,
    B.TotalMonthPlan,
    B.IdDiscountPlan,
    B.CreditsQty AS EmailQty,
    B.SubscribersQty
FROM
    [BillingCredits] B
INNER JOIN [UserTypesPlans] UTP ON UTP.IdUserTypePlan = B.IdUserTypePlan
INNER JOIN [User] U ON U.IdUser = B.IdUser
LEFT JOIN [Promotions] P ON P.IdPromotion = B.IdPromotion
WHERE
    b.IdUser = (SELECT IdUser FROM [User] WHERE Email = @email) AND U.IdCurrentBillingCredit IS NOT NULL
ORDER BY b.[Date] DESC;",
                new
                {
                    @email = accountName
                });

            return currentPlan;
        }

        public async Task<UserPlanInformation> GetFirstUpgrade(string accountName)
        {
            using var connection = _connectionFactory.GetConnection();

            var currentPlan = await connection.QueryFirstOrDefaultAsync<UserPlanInformation>(@"
SELECT
    B.[PlanFee] AS Fee,
    B.[CurrentMonthPlan],
    UTP.[IdUserType],
    B.[DiscountPlanFeeAdmin],
    B.[DiscountPlanFeePromotion],
    P.Code AS PromotionCode,
    B.IdUserTypePlan,
    B.Date
FROM
    [BillingCredits] B
INNER JOIN [UserTypesPlans] UTP ON UTP.IdUserTypePlan = B.IdUserTypePlan
INNER JOIN [User] U ON U.IdUser = B.IdUser
LEFT JOIN [Promotions] P ON P.IdPromotion = B.IdPromotion
WHERE
    b.IdUser = (SELECT IdUser FROM [User] WHERE Email = @email) AND IdBillingCreditType = 1
ORDER BY b.[Date] DESC;",
                new
                {
                    @email = accountName
                });

            return currentPlan;
        }

        public async Task<PlanDiscountInformation> GetDiscountInformation(int discountId)
        {
            using var connection = _connectionFactory.GetConnection();

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

        public async Task<int> GetAvailableCredit(string accountName)
        {
            using var connection = _connectionFactory.GetConnection();
            var partialBalance = await connection.QueryFirstOrDefaultAsync<int>(@"
SELECT
    MC.PartialBalance
FROM
    [dbo].[MovementsCredits] MC
INNER JOIN [User] U ON U.IdUser = MC.IdUser
WHERE
    U.Email = @email
ORDER BY
    MC.IdMovementCredit
DESC",
                new
                {
                    @email = accountName
                });

            return partialBalance;
        }

        public async Task<PlanInformation> GetChatPlanInformation(int chatPlanId)
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<PlanInformation>(@"
SELECT
    CP.[ConversationQty] AS ChatPlanConversationQty,
    CP.[Fee] AS ChatPlanFee
FROM
    [dbo].[ChatPlans] CP
WHERE
    CP.[IdChatPlan] = @chatPlanId",
    new { chatPlanId });

            return result.FirstOrDefault();
        }

        public async Task<BasePlanInformation> GetConversationPlanInformation(int conversationPlanId)
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<ConversationPlanInformation>(@"
SELECT  [IdChatPlan] AS PlanId,
        [ConversationQty] AS ConversationsQty,
        [Fee] AS Fee,
        [Agents] AS Agents,
        [Canales] AS Channels,
        2 AS PlanType
FROM [dbo].[ChatPlans]
WHERE [IdChatPlan] = @conversationPlanId",
    new { conversationPlanId });

            return result.FirstOrDefault();
        }

        public async Task<UserPlanInformation> GetCurrentPlanInformationWithAdditionalServices(string accountName)
        {
            using var connection = _connectionFactory.GetConnection();

            var currentPlan = await connection.QueryFirstOrDefaultAsync<UserPlanInformation>(@"
SELECT
    B.[PlanFee] AS Fee,
    B.[CurrentMonthPlan],
    UTP.[IdUserType],
    B.[DiscountPlanFeeAdmin],
    B.[DiscountPlanFeePromotion],
    P.Code AS PromotionCode,
    B.IdUserTypePlan,
    B.TotalMonthPlan,
    B.IdDiscountPlan,
    B.CreditsQty AS EmailQty,
    B.SubscribersQty,
    CP.Fee AS ChatPlanFee,
    CP.ConversationQty AS ChatPlanConversationQty
FROM
    [BillingCredits] B
INNER JOIN [UserTypesPlans] UTP ON UTP.IdUserTypePlan = B.IdUserTypePlan
INNER JOIN [User] U ON U.IdUser = B.IdUser
LEFT JOIN [Promotions] P ON P.IdPromotion = B.IdPromotion
LEFT JOIN [ChatPlanUsers] CUP ON CUP.IdBillingCredit = B.IdBillingCredit
LEFT JOIN [ChatPlans] CP ON CP.IdChatPlan = CUP.IdChatPlan
WHERE
    b.IdUser = (SELECT IdUser FROM [User] WHERE Email = @email) AND U.IdCurrentBillingCredit IS NOT NULL
ORDER BY b.[Date] DESC;",
                new
                {
                    @email = accountName
                });

            return currentPlan;
        }


        public async Task<BasePlanInformation> GetPlanInformation(PlanTypeEnum planType, int planId)
        {
            return planType switch
            {
                PlanTypeEnum.Marketing => new BasePlanInformation { PlanId = planId, PlanType = planType },
                PlanTypeEnum.Chat => await GetConversationPlanInformation(planId),
                _ => null,
            };
        }
    }
}
