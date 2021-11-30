using Doppler.AccountPlans.DopplerSecurity;
using Doppler.AccountPlans.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Doppler.AccountPlans.Encryption;
using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Utils;

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

        public AccountPlansController(
            ILogger<AccountPlansController> logger,
            IAccountPlansRepository accountPlansRepository,
            IDateTimeProvider dateTimeProvider,
            IPromotionRepository promotionRepository,
            IEncryptionService encryptionService)
        {
            _logger = logger;
            _accountPlansRepository = accountPlansRepository;
            _dateTimeProvider = dateTimeProvider;
            _promotionRepository = promotionRepository;
            _encryptionService = encryptionService;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/newplan/{newPlanId}/calculate")]
        public async Task<IActionResult> GetCalculateUpgradeCost(
            [FromRoute] string accountName,
            [FromRoute] int newPlanId,
            [FromQuery] int discountId,
            [FromQuery] string promocode = null)
        {
            _logger.LogInformation("Calculating plan amount details.");

            var newPlan = await _accountPlansRepository.GetPlanInformation(newPlanId);
            var currentPlan = await _accountPlansRepository.GetCurrentPlanInformation(accountName);
            var discountPlan = await _accountPlansRepository.GetDiscountInformation(discountId);

            if (newPlan == null)
                return new NotFoundResult();

            var promotion = new Promotion();
            if (!string.IsNullOrEmpty(promocode))
            {
                var encryptedCode = _encryptionService.EncryptAES256(promocode);
                promotion = await _promotionRepository.GetPromotionByCode(encryptedCode, newPlanId);
            }

            var upgradeCost = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, discountPlan, currentPlan, _dateTimeProvider.Now, promotion);

            return new OkObjectResult(upgradeCost);
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/plans/{planId}/validate/{promocode}")]
        public async Task<IActionResult> GetPromocodeInformation([FromRoute] int planId, [FromRoute] string promocode)
        {
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
    }
}
