using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.AccountPlans.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Doppler.AccountPlans.Controllers
{
    [Authorize]
    [ApiController]
    public class AccountPlansController
    {
        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/newplan/{planId}/calculate")]
        public string GetCalculateUpgradeCost([FromRoute] string accountName, [FromRoute] int planId, [FromQuery] string promocode = null)
        {
            return $"Hello! \"you\" that have access to the account name: '{accountName}' planId: '{planId}' and promocode:'{promocode}'";
        }
    }
}
