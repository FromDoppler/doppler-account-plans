using Dapper;
using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Helpers;
using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.TimeCollector;
using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public class AccountPlansRepository : IAccountPlansRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        private readonly ITimeCollector _timeCollector;

        public AccountPlansRepository(IDatabaseConnectionFactory connectionFactory, ITimeCollector timeCollector)
        {
            _connectionFactory = connectionFactory;
            _timeCollector = timeCollector;
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
    [DiscountXPlan] DP WITH(NOLOCK)
INNER JOIN [PaymentMethods] PM WITH(NOLOCK) ON DP.IdPaymentMethod = PM.IdPaymentMethod
WHERE
    DP.[IdUserTypePlan] = @planId AND PM.PaymentMethodName = @paymentMethod AND DP.Active = 1
ORDER BY
    DP.[MonthPlan]",
    new { planId, paymentMethod });

            return result;
        }

        public async Task<PlanInformation> GetPlanInformation(int planId)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<PlanInformation>(@"
SELECT
    UTP.[IdUserType],
    UTP.[EmailQty],
    UTP.[Fee],
    UTP.[SubscribersQty],
    UTP.[PlanType] AS Type
FROM
    [UserTypesPlans] UTP WITH(NOLOCK)
WHERE
    UTP.[IdUserTypePlan] = @planId",
    new { planId });

            return result.FirstOrDefault();
        }

        public async Task<UserPlanInformation> GetCurrentPlanInformation(string accountName)
        {
            using var _ = _timeCollector.StartScope();
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
    [BillingCredits] B WITH(NOLOCK)
INNER JOIN [UserTypesPlans] UTP WITH(NOLOCK) ON UTP.IdUserTypePlan = B.IdUserTypePlan
INNER JOIN [User] U WITH(NOLOCK) ON U.IdUser = B.IdUser
LEFT JOIN [Promotions] P WITH(NOLOCK) ON P.IdPromotion = B.IdPromotion
WHERE
    b.IdUser = (SELECT IdUser FROM [User] WITH(NOLOCK) WHERE Email = @email) AND U.IdCurrentBillingCredit IS NOT NULL
ORDER BY b.[Date] DESC;",
                new
                {
                    @email = accountName
                });

            return currentPlan;
        }

        public async Task<UserPlanInformation> GetFirstUpgrade(string accountName)
        {
            using var _ = _timeCollector.StartScope();
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
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

            var discountPlan = await connection.QueryFirstOrDefaultAsync<PlanDiscountInformation>(@"
SELECT
    d.[MonthPlan],
    d.[DiscountPlanFee],
    d.[ApplyPromo]
FROM
    DiscountXPlan d  WITH(NOLOCK)
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
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var partialBalance = await connection.QueryFirstOrDefaultAsync<int>(@"
SELECT
    MC.PartialBalance
FROM
    [dbo].[MovementsCredits] MC  WITH(NOLOCK)
INNER JOIN [User] U  WITH(NOLOCK) ON U.IdUser = MC.IdUser
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
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<PlanInformation>(@"
SELECT
    CP.[ConversationQty] AS ChatPlanConversationQty,
    CP.[Fee] AS ChatPlanFee
FROM
    [dbo].[ChatPlans] CP  WITH(NOLOCK)
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
        [Description] as Description,
        [ConversationQty] AS ConversationsQty,
        [Fee] AS Fee,
        [Agents] AS Agents,
        [Canales] AS Channels,
        2 AS PlanType
FROM [dbo].[ChatPlans]  WITH(NOLOCK)
WHERE [IdChatPlan] = @conversationPlanId",
    new { conversationPlanId });

            return result.FirstOrDefault();
        }

        public async Task<UserPlanInformation> GetCurrentPlanInformationWithAdditionalServices(string accountName)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

            var currentPlan = await connection.QueryFirstOrDefaultAsync<UserPlanInformation>(@$"
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
    [BillingCredits] B  WITH(NOLOCK)
INNER JOIN [UserTypesPlans] UTP WITH(NOLOCK) ON UTP.IdUserTypePlan = B.IdUserTypePlan
INNER JOIN [User] U WITH(NOLOCK) ON U.IdUser = B.IdUser
LEFT JOIN [Promotions] P WITH(NOLOCK) ON P.IdPromotion = B.IdPromotion
LEFT JOIN [UserAddOn] UAO WITH(NOLOCK) ON UAO.IdUser = U.IdUser AND UAO.IdAddOnType = 2
LEFT JOIN [BillingCredits] CPBC ON CPBC.IdBillingCredit = UAO.IdCurrentBillingCredit AND CPBC.IdBillingCreditType != {(int)BillingCreditTypeEnum.Conversation_Canceled}
LEFT JOIN [ChatPlanUsers] CUP WITH(NOLOCK) ON CUP.IdBillingCredit = CPBC.IdBillingCredit
LEFT JOIN [ChatPlans] CP WITH(NOLOCK) ON CP.IdChatPlan = CUP.IdChatPlan
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

        public async Task<IEnumerable<ConversationPlanInformation>> GetConversationPlans()
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<ConversationPlanInformation>(@"
SELECT  [IdChatPlan] AS PlanId,
        [Description] as Description,
        [ConversationQty] AS ConversationsQty,
        [Fee] AS Fee,
        [Agents] AS Agents,
        [Canales] AS Channels,
        [AdditionalConversation] AS AdditionalConversation,
        [AdditionalAgent] AS AdditionalAgent,
        [AdditionalChannel] AS AdditionalChannel,
        2 AS PlanType
FROM [dbo].[ChatPlans] WITH(NOLOCK)
WHERE [Active] = 1 AND [Fee] > 0
ORDER BY ConversationsQty");

            return result;
        }

        public async Task<IEnumerable<ConversationPlanInformation>> GetCustomConversationPlans()
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<ConversationPlanInformation>(@"
SELECT  [IdChatPlan] AS PlanId,
        [Description] as Description,
        [ConversationQty] AS ConversationsQty,
        [Fee] AS Fee,
        [Agents] AS Agents,
        [Canales] AS Channels,
        [AdditionalConversation] AS AdditionalConversation,
        [AdditionalAgent] AS AdditionalAgent,
        [AdditionalChannel] AS AdditionalChannel,
        2 AS PlanType
FROM [dbo].[ChatPlans] WITH(NOLOCK)
WHERE [Active] = 0 AND [Fee] > 0
ORDER BY ConversationsQty");

            return result;
        }

        public async Task<IEnumerable<LandingPlanInformation>> GetLandingPlans()
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<LandingPlanInformation>(@"
SELECT  [IdLandingPlan] AS PlanId,
        [Description],
        [LandingQty] AS LandingsQty,
        [Fee] AS Fee,
        3 AS PlanType
FROM [dbo].[LandingPlan] WITH(NOLOCK)
WHERE [Active] = 1
ORDER BY Fee");

            return result;
        }

        public async Task<UserPlanInformation> GetLastLandingPlanBillingInformation(string accountName)
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryFirstOrDefaultAsync<UserPlanInformation>(@$"SELECT
    BC.[PlanFee] AS Fee,
    BC.[CurrentMonthPlan],
    BC.[DiscountPlanFeeAdmin],
    BC.[DiscountPlanFeePromotion],
    BC.IdUserTypePlan,
    BC.TotalMonthPlan,
    BC.IdDiscountPlan,
    BC.CreditsQty AS EmailQty,
    BC.SubscribersQty
FROM [UserAddOn] UA
INNER JOIN [BillingCredits] BC ON BC.IdBillingCredit = UA.IdCurrentBillingCredit
INNER JOIN [User] U ON U.IdUser = UA.IdUser
WHERE
    U.IdCurrentBillingCredit IS NOT NULL AND
    UA.IdAddOnType =  1 AND
    (BC.IdBillingCreditType IN ({(int)BillingCreditTypeEnum.Landing_Buyed_CC},
                                {(int)BillingCreditTypeEnum.Landing_Request},
                                {(int)BillingCreditTypeEnum.Downgrade_Between_Landings},
                                {(int)BillingCreditTypeEnum.Landing_New_Month})) AND
    UA.IdUser = (SELECT IdUser FROM [User] WHERE Email = @email)
ORDER BY BC.[Date] DESC", new
            {
                @email = accountName
            });

            return result;
        }

        public async Task<UserPlanInformation> GetFirstLandingUpgrade(string accountName)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

            var currentPlan = await connection.QueryFirstOrDefaultAsync<UserPlanInformation>(@$"
SELECT
    B.[PlanFee] AS Fee,
    B.[CurrentMonthPlan],
    B.[DiscountPlanFeeAdmin],
    B.[DiscountPlanFeePromotion],
    P.Code AS PromotionCode,
    B.IdUserTypePlan,
    B.Date
FROM
    [BillingCredits] B
INNER JOIN [User] U ON U.IdUser = B.IdUser
LEFT JOIN [Promotions] P ON P.IdPromotion = B.IdPromotion
WHERE
    b.IdUser = (SELECT IdUser FROM [User] WHERE Email = @email) AND  IdBillingCreditType IN ({(int)BillingCreditTypeEnum.Landing_Request}, {(int)BillingCreditTypeEnum.Landing_Buyed_CC})
ORDER BY b.[Date] ASC;",
                new
                {
                    @email = accountName
                });

            return currentPlan;
        }

        public async Task<DateTime?> GetFirstUpgradeDate(string accountName)
        {
            using var connection = _connectionFactory.GetConnection();

            int? idClientManager = await connection.QueryFirstOrDefaultAsync<int?>(@"
            SELECT
                [IdClientManager]
            FROM [dbo].[User]
            WHERE Email = @accountName",
                new
                {
                    @accountName = accountName
                });

            if (idClientManager is not null)
            {
                var upgradeDate = await connection.QueryFirstOrDefaultAsync<DateTime>(@"
                SELECT
                    [UTCUpgradeDate]
                FROM [dbo].[ClientManagerUpgrade]
                WHERE [IdClientManager] = @idClientManager
                ORDER BY [UTCUpgradeDate] DESC;",
                    new
                    {
                        @idClientManager = idClientManager
                    });

                return upgradeDate;
            }
            else
            {
                var upgradeDate = await connection.QueryFirstOrDefaultAsync<DateTime>(@"
                SELECT
                    [Date]
                FROM
                    [BillingCredits]
                WHERE
                    IdUser = (SELECT IdUser FROM [User] WHERE Email = @accountName) AND IdBillingCreditType = 1
                ORDER BY [Date] DESC;",
                    new
                    {
                        @accountName = accountName
                    });

                return upgradeDate;
            }
        }

        public async Task<IEnumerable<OnSitePlanInformation>> GetOnSitePlans()
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<OnSitePlanInformation>(@"
SELECT  [IdOnSitePlan] AS PlanId,
        [Description] AS [Description],
        [PrintQty] AS PrintQty,
        [Fee] AS Fee,
        [AdditionalPrint] AS AdditionalPrint,
        [AdditionalPrint] AS Additional,
        [PrintQty] AS Quantity,
        4 AS PlanType
FROM [dbo].[OnSitePlan] WITH(NOLOCK)
WHERE [Active] = 1 AND [Fee] > 0
ORDER BY [PrintQty]");

            return result;
        }

        public async Task<PlanInformation> GetOnSitePlanInformation(int onSitePlanId)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<PlanInformation>(@"
SELECT
    OSP.[PrintQty] AS PrintQty,
    OSP.[Fee] AS ChatPlanFee
FROM
    [dbo].[OnSitePlan] OSP  WITH(NOLOCK)
WHERE
    OSP.[IdOnSitePlan] = @onSitePlanId",
    new { onSitePlanId });

            return result.FirstOrDefault();
        }

        public async Task<UserPlan> GetCurrentPlanWithAdditionalServices(string accountName)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();

            var currentPlan = await connection.QueryFirstOrDefaultAsync<UserPlan>(@$"
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
    B.IdUser
FROM
    [BillingCredits] B  WITH(NOLOCK)
INNER JOIN [UserTypesPlans] UTP WITH(NOLOCK) ON UTP.IdUserTypePlan = B.IdUserTypePlan
INNER JOIN [User] U WITH(NOLOCK) ON U.IdUser = B.IdUser
LEFT JOIN [Promotions] P WITH(NOLOCK) ON P.IdPromotion = B.IdPromotion
WHERE
    b.IdUser = (SELECT IdUser FROM [User] WHERE Email = @email) AND U.IdCurrentBillingCredit IS NOT NULL
ORDER BY b.[Date] DESC;",
                new
                {
                    @email = accountName
                });

            if (currentPlan != null)
            {
                var additionalServices = await connection.QueryAsync<AdditionalService>(@$"
SELECT
    UAO.IdAddOnType,
    CASE
        WHEN UAO.IdAddOnType = 1
            THEN LPBC.PlanFee
        WHEN UAO.IdAddOnType = 2
            THEN CPBC.PlanFee
        WHEN UAO.IdAddOnType = 3
            THEN  OSBC.PlanFee
        WHEN UAO.IdAddOnType = 4
            THEN PNBC.PlanFee
    ELSE 0
    END AS Fee,
    CASE
        WHEN UAO.IdAddOnType = 1
            THEN SUM(LP.LandingQty * LPU.PackQty)
        WHEN UAO.IdAddOnType = 2
            THEN SUM(CP.ConversationQty)
        WHEN UAO.IdAddOnType = 3
            THEN  SUM(OSP.PrintQty)
        WHEN UAO.IdAddOnType = 4
            THEN  SUM(PNP.Quantity)
        ELSE 0
    END AS Qty
FROM [UserAddOn] UAO
/* Landings plans */
LEFT JOIN [BillingCredits] LPBC ON LPBC.IdBillingCredit = UAO.IdCurrentBillingCredit AND UAO.IdAddOnType = 1 AND LPBC.IdBillingCreditType IN (23, 24, 26, 27)
LEFT JOIN [LandingPlanUser] LPU ON LPU.IdBillingCredit = LPBC.IdBillingCredit
LEFT JOIN [LandingPlan] LP ON LP.IdLandingPlan = LPU.IdLandingPlan

/* Conversation plans */
LEFT JOIN [BillingCredits] CPBC ON CPBC.IdBillingCredit = UAO.IdCurrentBillingCredit AND UAO.IdAddOnType = 2 AND CPBC.IdBillingCreditType IN (28, 29, 31, 32)
LEFT JOIN [ChatPlanUsers] CPU ON CPU.IdBillingCredit = CPBC.IdBillingCredit
LEFT JOIN [ChatPlans] CP ON CP.IdChatPlan = CPU.IdChatPlan

/* OnSite plans */
LEFT JOIN [BillingCredits] OSBC ON OSBC.IdBillingCredit = UAO.IdCurrentBillingCredit AND UAO.IdAddOnType = 3 AND OSBC.IdBillingCreditType IN (34, 35, 37, 38)
LEFT JOIN [OnSitePlanUser] OSPU ON OSPU.IdBillingCredit = OSBC.IdBillingCredit
LEFT JOIN [OnSitePlan] OSP ON OSP.IdOnSItePlan = OSPU.IdOnSItePlan

/* Push Notification plans */
LEFT JOIN [BillingCredits] PNBC ON PNBC.IdBillingCredit = UAO.IdCurrentBillingCredit AND UAO.IdAddOnType = 4 AND PNBC.IdBillingCreditType IN (40, 41, 43, 44)
LEFT JOIN [PushNotificationPlanUser] PNPU ON PNBC.IdBillingCredit = PNBC.IdBillingCredit
LEFT JOIN [PushNotificationPlan] PNP ON PNP.IdPushNotificationPlan = PNPU.IdPushNotificationPlan
WHERE UAO.IdUser = @userId
GROUP BY UAO.IdAddOnType ,
        CASE
            WHEN UAO.IdAddOnType = 1
                THEN LPBC.PlanFee
            WHEN UAO.IdAddOnType = 2
                THEN CPBC.PlanFee
            WHEN UAO.IdAddOnType = 3
                THEN OSBC.PlanFee
            WHEN UAO.IdAddOnType = 4
                THEN PNBC.PlanFee
            ELSE 0
        END",
                    new
                    {
                        @userId = currentPlan.IdUser
                    });

                currentPlan.AdditionalServices = additionalServices.ToList();


                return currentPlan;
            }

            return null;
        }

        public async Task<IEnumerable<OnSitePlanInformation>> GetCustomOnSitePlans()
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<OnSitePlanInformation>(@"
SELECT  [IdOnSitePlan] AS PlanId,
        [Description] AS [Description],
        [PrintQty] AS PrintQty,
        [Fee] AS Fee,
        [AdditionalPrint] AS AdditionalPrint,
        [AdditionalPrint] AS Additional,
        [PrintQty] AS Quantity,
        4 AS PlanType
FROM [dbo].[OnSitePlan] WITH(NOLOCK)
WHERE [Active] = 0 AND [Fee] > 0
ORDER BY [PrintQty]");

            return result;
        }

        public async Task<AddOnPlan> GetOnSitePlanById(int onSitePlanId)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<AddOnPlan>(@"
SELECT [IdOnSitePlan] AS PlanId
        ,[Description]
        ,[PrintQty] AS [Quantity]
        ,[Fee]
        ,[AdditionalPrint] AS [Additional]
        ,3 AS AddOnType
FROM [dbo].[OnSitePlan]
WHERE [IdOnSitePlan] = @onSitePlanId",
    new { onSitePlanId });

            return result.FirstOrDefault();
        }

        public async Task<AddOnPlan> GetPushNotificationPlanById(int pushNotificationPlanId)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<AddOnPlan>(@"
SELECT [IdPushNotificationPlan] AS PlanId
        ,[Description]
        ,[Quantity]
        ,[Fee]
        ,[Additional]
        ,4 AS AddOnType
FROM [dbo].[PushNotificationPlan]
WHERE [IdPushNotificationPlan] = @pushNotificationPlanId",
    new { pushNotificationPlanId });

            return result.FirstOrDefault();
        }

        public async Task<PlanInformation> GetPushNotificationPlanInformation(int pushNotificationPlanId)
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<PlanInformation>(@"
SELECT
    [Quantity] AS PrintQty,
    [Fee] AS ChatPlanFee
FROM [dbo].[PushNotificationPlan]
WHERE [IdPushNotificationPlan] = @pushNotificationPlanId",
    new { pushNotificationPlanId });

            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<PushNotificationPlanInformation>> GetPushNotificationPlans()
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<PushNotificationPlanInformation>(@"
SELECT [IdPushNotificationPlan] AS PlanId
        ,[Description]
        ,[Quantity]
        ,[Fee]
        ,[Additional]
        ,5 AS PlanType
FROM [dbo].[PushNotificationPlan]
WHERE [Active] = 1 AND [Fee] > 0");

            return result;
        }

        public async Task<IEnumerable<PushNotificationPlanInformation>> GetCustomPushNotificationPlans()
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<PushNotificationPlanInformation>(@"
SELECT [IdPushNotificationPlan] AS PlanId
        ,[Description]
        ,[Quantity]
        ,[Fee]
        ,[Additional]
        ,5 AS PlanType
FROM [dbo].[PushNotificationPlan]
WHERE [Custom] = 1 AND [Fee] > 0");

            return result;
        }

        public async Task<ConversationPlan> GetFreeConversationPlan()
        {
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<ConversationPlan>(@"
SELECT  [IdChatPlan] AS PlanId,
        [Description] as Description,
        [ConversationQty] AS Quantity,
        [Fee] AS Fee,
        [Agents] AS Agents,
        [Canales] AS Channels,
        [AdditionalConversation] AS AdditionalConversation,
        [AdditionalAgent] AS AdditionalAgent,
        [AdditionalChannel] AS AdditionalChannel,
        2 AS PlanType
FROM [dbo].[ChatPlans] WITH(NOLOCK)
WHERE [Fee] = 0
ORDER BY ConversationQty");

            return result.FirstOrDefault();
        }

        public async Task<AddOnPlan> GetFreeOnSitePlan()
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<AddOnPlan>(@"
SELECT [IdOnSitePlan] AS PlanId
        ,[Description]
        ,[PrintQty] AS [Quantity]
        ,[Fee]
        ,[AdditionalPrint] AS [Additional]
        ,3 AS AddOnType
FROM [dbo].[OnSitePlan]
WHERE [Fee] = 0");

            return result.FirstOrDefault();
        }

        public async Task<AddOnPlan> GetFreePushNotificationPlan()
        {
            using var _ = _timeCollector.StartScope();
            using var connection = _connectionFactory.GetConnection();
            var result = await connection.QueryAsync<AddOnPlan>(@"
SELECT [IdPushNotificationPlan] AS PlanId
        ,[Description]
        ,[Quantity]
        ,[Fee]
        ,[Additional]
        ,4 AS AddOnType
FROM [dbo].[PushNotificationPlan]
WHERE [Fee] = 0");

            return result.FirstOrDefault();
        }
    }
}
