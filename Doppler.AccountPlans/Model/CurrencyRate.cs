using System;

namespace Doppler.AccountPlans.Model
{
    public class CurrencyRate
    {
        public int IdCurrencyRate { get; set; }
        public int IdCurrencyTypeFrom { get; set; }
        public int IdCurrencyTypeTo { get; set; }
        public decimal? Rate { get; set; }
        public DateTime UTCFromDate { get; set; }
        public bool Active { get; set; }
    }
}
