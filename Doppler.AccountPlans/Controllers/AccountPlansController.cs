using Doppler.AccountPlans.DopplerSecurity;
using Doppler.AccountPlans.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
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

        public AccountPlansController(
            ILogger<AccountPlansController> logger,
            IAccountPlansRepository accountPlansRepository,
            IDateTimeProvider dateTimeProvider)
        {
            _logger = logger;
            _accountPlansRepository = accountPlansRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/newplan/{newPlanId}/calculate")]
        public async Task<IActionResult> GetCalculateUpgradeCost([FromRoute] string accountName, [FromRoute] int newPlanId, [FromQuery] int discountId, [FromQuery] string promocode = null)
        {
            _logger.LogInformation("Calculating plan amount details.");

            var newPlan = await _accountPlansRepository.GetPlanInformation(newPlanId);
            var currentPlan = await _accountPlansRepository.GetCurrentPlanInformation(accountName);
            var discountPlan = await _accountPlansRepository.GetDiscountInformation(discountId);

            if (newPlan == null)
                return new NotFoundResult();

            var upgradeCost = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, discountPlan, currentPlan, _dateTimeProvider.Now);
            return new OkObjectResult(upgradeCost);
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
