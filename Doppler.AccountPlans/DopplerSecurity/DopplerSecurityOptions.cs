using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Doppler.AccountPlans.DopplerSecurity
{
    public class DopplerSecurityOptions
    {
        public IEnumerable<SecurityKey> SigningKeys { get; set; } = new SecurityKey[0];
    }
}
