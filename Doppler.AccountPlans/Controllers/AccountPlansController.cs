using Doppler.AccountPlans.DopplerSecurity;
using Doppler.AccountPlans.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Doppler.AccountPlans.Encryption;
using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Utils;
using Doppler.AccountPlans.Enums;
using System;
using System.Numerics;
using Doppler.AccountPlans.TimeCollector;
using System.Collections.Generic;

namespace Doppler.AccountPlans.Controllers
{
    [Authorize]
    [ApiController]
    public class AccountPlansController
    {
        private readonly ILogger _logger;
        private readonly IAccountPlansRepository _accountPlansRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly ITimeCollector _timeCollector;

        public AccountPlansController(
            ILogger<AccountPlansController> logger,
            IAccountPlansRepository accountPlansRepository,
            IDateTimeProvider dateTimeProvider,
            IPromotionRepository promotionRepository,
            IEncryptionService encryptionService,
            ITimeCollector timeCollector)
        {
            _logger = logger;
            _accountPlansRepository = accountPlansRepository;
            _dateTimeProvider = dateTimeProvider;
            _promotionRepository = promotionRepository;
            _encryptionService = encryptionService;
            _timeCollector = timeCollector;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/newplan/{newPlanType}/{newPlanId}/calculate-amount")]
        public async Task<IActionResult> GetCalculateUpgradeCostWithAddOns(
            [FromRoute] string accountName,
            [FromRoute] int newPlanId,
            [FromRoute] PlanTypeEnum newPlanType,
            [FromQuery] int discountId,
            [FromQuery] string promocode = null)
        {
            using var _ = _timeCollector.StartScope();

            _logger.LogInformation("Calculating plan amount details.");

            PlanInformation newPlan = null;

            switch (newPlanType)
            {
                case PlanTypeEnum.Marketing:
                    newPlan = await _accountPlansRepository.GetPlanInformation(newPlanId);
                    break;
                case PlanTypeEnum.Chat:
                    newPlan = await _accountPlansRepository.GetChatPlanInformation(newPlanId);
                    break;
                case PlanTypeEnum.OnSite:
                    newPlan = await _accountPlansRepository.GetOnSitePlanInformation(newPlanId);
                    break;
                default:
                    newPlan = null;
                    break;
            }

            if (newPlan == null)
                return new NotFoundResult();

            var currentPlan = await _accountPlansRepository.GetCurrentPlanWithAdditionalServices(accountName);
            var discountPlan = await _accountPlansRepository.GetDiscountInformation(discountId);

            var promotion = new Promotion();
            TimesApplyedPromocode timesAppliedPromocode = null;
            Promotion currentPromotion = null;
            DateTime? firstUpgradeDate = null;
            PlanDiscountInformation currentDiscountPlan = null;
            decimal totalCreditDiscount = 0;

            if (!string.IsNullOrEmpty(promocode))
            {
                var encryptedCode = _encryptionService.EncryptAES256(promocode);
                promotion = await _promotionRepository.GetPromotionByCode(encryptedCode, newPlanId);
            }

            if (currentPlan != null)
            {
                if (currentPlan.IdUserType != Enums.UserTypesEnum.Individual)
                {
                    currentPromotion = await _promotionRepository.GetPromotionByCode(currentPlan.PromotionCode, newPlanId);
                    timesAppliedPromocode = await _promotionRepository.GetHowManyTimesApplyedPromocode(currentPlan.PromotionCode, accountName);
                }
                else
                {
                    if (currentPlan.IdUserType == UserTypesEnum.Individual && newPlan.IdUserType != UserTypesEnum.Individual)
                    {
                        var prepaidPromotion = await _promotionRepository.GetPromotionByCode(currentPlan.PromotionCode, currentPlan.IdUserTypePlan);

                        var availableCredits = await _accountPlansRepository.GetAvailableCredit(accountName);
                        var credits = availableCredits > currentPlan.EmailQty ? currentPlan.EmailQty : availableCredits;
                        var priceByCredit = currentPlan.EmailQty > 0 ? currentPlan.Fee / currentPlan.EmailQty : 0;

                        decimal creditsDiscount = credits * priceByCredit;
                        totalCreditDiscount = creditsDiscount - (prepaidPromotion != null && prepaidPromotion.DiscountPercentage != null ? Math.Round(creditsDiscount * prepaidPromotion.DiscountPercentage.Value / 100, 2) : 0);
                    }
                }

                //firstUpgrade = await _accountPlansRepository.GetFirstUpgrade(accountName);
                firstUpgradeDate = await _accountPlansRepository.GetFirstUpgradeDate(accountName);
                currentDiscountPlan = await _accountPlansRepository.GetDiscountInformation(currentPlan.IdDiscountPlan);
            }

            /* Marketing plan */
            var upgradeCost = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, discountPlan, currentPlan, _dateTimeProvider.Now, promotion, timesAppliedPromocode, currentPromotion, firstUpgradeDate, currentDiscountPlan, totalCreditDiscount, newPlanType);

            return new OkObjectResult(upgradeCost);
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/newplan/landingplan/calculate")]
        public async Task<IActionResult> GetCalculateUpgradeLandingPlanCost(
            [FromRoute] string accountName,
            [FromQuery] string landingIds,
            [FromQuery] string landingPacks)
        {
            if (string.IsNullOrEmpty(landingIds))
            {
                return new BadRequestObjectResult(new { message = "The 'landingIds' query parameter is required" });
            }

            if (string.IsNullOrEmpty(landingPacks))
            {
                return new BadRequestObjectResult(new { message = "The 'landingPacks' query parameter is required" });
            }

            var currentPlan = await _accountPlansRepository.GetCurrentPlanInformation(accountName);

            if (currentPlan is null)
            {
                return new BadRequestObjectResult(new { message = "The given user has no billing credit" });
            }

            var landingPlans = await _accountPlansRepository.GetLandingPlans();
            var discountPlan = currentPlan.IdDiscountPlan > 0 ? await _accountPlansRepository.GetDiscountInformation(currentPlan.IdDiscountPlan) : null;
            var landingPlansSummary = new List<LandingPlanSummary>();

            var landingIdsList = landingIds.Split(",", StringSplitOptions.RemoveEmptyEntries);
            var landingPacksList = landingPacks.Split(",", StringSplitOptions.RemoveEmptyEntries);

            if (landingIdsList.Length != landingPacksList.Length)
            {
                return new BadRequestObjectResult(new { message = "The number of landing ids and landing pakcs must be the same" });
            }

            for (int i = 0; i < landingIdsList.Length; i++)
            {
                if (int.TryParse(landingIdsList[i], out int idPlan) && int.TryParse(landingPacksList[i], out int numberOfPlan))
                {
                    landingPlansSummary.Add(new LandingPlanSummary { IdLandingPlan = idPlan, NumberOfPlans = numberOfPlan });
                }
            }

            var lastLandingPlan = await _accountPlansRepository.GetLastLandingPlanBillingInformation(accountName);
            var firstUpgradeDate = await _accountPlansRepository.GetFirstUpgradeDate(accountName);
            var upgradeCost = CalculateUpgradeCostHelper.CalculateLandingPlanAmountDetails(currentPlan, _dateTimeProvider.Now, landingPlansSummary, landingPlans, discountPlan, lastLandingPlan, firstUpgradeDate);

            return new OkObjectResult(upgradeCost);
        }

        [HttpGet("/plans/{planId}/validate/{promocode}")]
        public async Task<IActionResult> GetPromocodeInformation([FromRoute] int planId, [FromRoute] string promocode)
        {
            using var _ = _timeCollector.StartScope();

            var encryptedCode = _encryptionService.EncryptAES256(promocode);
            var promotion = await _promotionRepository.GetPromotionByCode(encryptedCode, planId);

            if (promotion == null)
                return new NotFoundResult();

            return new OkObjectResult(promotion);
        }

        [HttpGet("/plans/{planId}/{paymentMethod}/discounts")]
        public async Task<IActionResult> GetPlanDiscountInformation([FromRoute] int planId, [FromRoute] string paymentMethod)
        {
            var contactInformation = await _accountPlansRepository.GetPlanDiscountInformation(planId, paymentMethod);

            if (contactInformation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(contactInformation);
        }

        [HttpGet("/plans/{planId}")]
        public async Task<IActionResult> GetPlanInformation([FromRoute] int planId)
        {
            var contactInformation = await _accountPlansRepository.GetPlanInformation(planId);

            if (contactInformation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(contactInformation);
        }

        [HttpGet("/{planType}/plans/{planId}")]
        public async Task<IActionResult> GetPlanInformationByPlanIdAndType([FromRoute] int planId, [FromRoute] int planType)
        {
            PlanInformation planInformation = null;

            switch (planType)
            {
                case (int)PlanTypeEnum.Marketing:
                    planInformation = await _accountPlansRepository.GetPlanInformation(planId);
                    break;
                case (int)PlanTypeEnum.Chat:
                    planInformation = await _accountPlansRepository.GetChatPlanInformation(planId);
                    break;
                case (int)PlanTypeEnum.OnSite:
                    planInformation = await _accountPlansRepository.GetOnSitePlanInformation(planId);
                    break;
                default:
                    planInformation = null;
                    break;
            }

            if (planInformation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(planInformation);
        }

        [HttpGet("/plans/{planType}/{planId}")]
        public async Task<IActionResult> GetPlanInformationByTypeAndId([FromRoute] PlanTypeEnum planType, [FromRoute] int planId)
        {
            var planInformation = await _accountPlansRepository.GetPlanInformation(planType, planId);

            if (planInformation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(planInformation);
        }

        [HttpGet("/conversation-plans")]
        public async Task<IActionResult> GetConversationPlans()
        {
            var planInformation = await _accountPlansRepository.GetConversationPlans();

            if (planInformation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(planInformation);
        }

        [HttpGet("/custom-conversation-plans")]
        public async Task<IActionResult> GetCustomConversationPlans()
        {
            var planInformation = await _accountPlansRepository.GetCustomConversationPlans();

            if (planInformation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(planInformation);
        }

        [HttpGet("/landing-plans")]
        public async Task<IActionResult> GetLandingPlans()
        {
            var planInformation = await _accountPlansRepository.GetLandingPlans();

            if (planInformation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(planInformation);
        }

        [HttpGet("/onsite-plans")]
        public async Task<IActionResult> GetOnSitePlans()
        {
            var planInformation = await _accountPlansRepository.GetOnSitePlans();

            if (planInformation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(planInformation);
        }

        [HttpGet("/custom-onsite-plans")]
        public async Task<IActionResult> GetCustomOnSitePlans()
        {
            var planInformation = await _accountPlansRepository.GetCustomOnSitePlans();

            if (planInformation == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(planInformation);
        }
    }
}
