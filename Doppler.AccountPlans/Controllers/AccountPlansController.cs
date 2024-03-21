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
        [HttpGet("/accounts/{accountName}/newplan/{newPlanId}/calculate")]
        public async Task<IActionResult> GetCalculateUpgradeCost(
            [FromRoute] string accountName,
            [FromRoute] int newPlanId,
            [FromQuery] int discountId,
            [FromQuery] string promocode = null)
        {
            using var _ = _timeCollector.StartScope();

            _logger.LogInformation("Calculating plan amount details.");

            var newPlan = await _accountPlansRepository.GetPlanInformation(newPlanId);
            var currentPlan = await _accountPlansRepository.GetCurrentPlanInformation(accountName);
            var discountPlan = await _accountPlansRepository.GetDiscountInformation(discountId);

            if (newPlan == null)
                return new NotFoundResult();

            var promotion = new Promotion();
            TimesApplyedPromocode timesAppliedPromocode = null;
            Promotion currentPromotion = null;
            UserPlanInformation firstUpgrade = null;
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

                firstUpgrade = await _accountPlansRepository.GetFirstUpgrade(accountName);
                currentDiscountPlan = await _accountPlansRepository.GetDiscountInformation(currentPlan.IdDiscountPlan);
            }

            var upgradeCost = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, discountPlan, currentPlan, _dateTimeProvider.Now, promotion, timesAppliedPromocode, currentPromotion, firstUpgrade, currentDiscountPlan, totalCreditDiscount, PlanTypeEnum.Marketing);

            return new OkObjectResult(upgradeCost);
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/newplan/{newPlanType}/{newPlanId}/calculate-amount")]
        public async Task<IActionResult> GetCalculateUpgradeCostWithChatPlan(
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
                default:
                    newPlan = null;
                    break;
            }

            if (newPlan == null)
                return new NotFoundResult();

            var currentPlan = await _accountPlansRepository.GetCurrentPlanInformationWithAdditionalServices(accountName);
            var discountPlan = await _accountPlansRepository.GetDiscountInformation(discountId);

            var promotion = new Promotion();
            TimesApplyedPromocode timesAppliedPromocode = null;
            Promotion currentPromotion = null;
            UserPlanInformation firstUpgrade = null;
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

                firstUpgrade = await _accountPlansRepository.GetFirstUpgrade(accountName);
                currentDiscountPlan = await _accountPlansRepository.GetDiscountInformation(currentPlan.IdDiscountPlan);
            }

            /* Marketing plan */
            var upgradeCost = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, discountPlan, currentPlan, _dateTimeProvider.Now, promotion, timesAppliedPromocode, currentPromotion, firstUpgrade, currentDiscountPlan, totalCreditDiscount, newPlanType);

            return new OkObjectResult(upgradeCost);
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/landingplan")]
        public async Task<IActionResult> GetCalculateUpgradeLandingPlanCost(
            [FromQuery] Dictionary<string, string> landingPlanParams,
            [FromQuery] int discountId)
        {
            var landingPlans = await _accountPlansRepository.GetLandingPlans();
            var discountPlan = discountId > 0 ? await _accountPlansRepository.GetDiscountInformation(discountId) : null;
            var landingPlansSummary = new List<LandingPlanSummary>();

            foreach (var param in landingPlanParams)
            {
                if (int.TryParse(param.Key, out int idPlan) && int.TryParse(param.Value, out int numberOfPlan))
                {
                    landingPlansSummary.Add(new LandingPlanSummary { IdLandingPlan = idPlan, NumberOfPlans = numberOfPlan });
                }
            }

            var totalFee = CalculateUpgradeCostHelper.CalculateLandingPlanAmountDetails(landingPlansSummary, landingPlans, discountPlan);

            return new OkObjectResult(new { totalFee });
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
    }
}
