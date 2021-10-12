using Doppler.AccountPlans.DopplerSecurity;
using Doppler.AccountPlans.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Controllers
{
    [Authorize]
    [ApiController]
    public class AccountPlansController
    {
        private readonly ILogger _logger;
        private readonly IAccountPlansRepository _accountPlansRepository;

        public AccountPlansController(
            ILogger<AccountPlansController> logger,
            IAccountPlansRepository accountPlansRepository)
        {
            _logger = logger;
            _accountPlansRepository = accountPlansRepository;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/newplan/{newPlanId}/calculate/{paymentMethod?}/{country?}")]
        public async Task<IActionResult> GetCalculateUpgradeCost([FromRoute] string accountName, [FromRoute] int newPlanId, [FromQuery] int discountId, [FromQuery] string promocode = null, [FromRoute] string paymentMethod = "", [FromRoute] string country = "")
        {
            _logger.LogInformation("Calculating plan amount details.");

            var planDetails = await _accountPlansRepository.GetPlanAmountDetails(newPlanId, accountName, discountId, paymentMethod, country);

            if (planDetails == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(planDetails);
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
