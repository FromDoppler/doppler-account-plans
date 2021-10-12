using System.Collections.Generic;

namespace Doppler.AccountPlans.Infrastructure
{
    public class TaxesSettings
    {
        public IDictionary<string, IList<TaxSettings>> Taxes { get; set; }
    }
}
