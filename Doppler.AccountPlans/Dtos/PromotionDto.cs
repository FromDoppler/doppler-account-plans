using Doppler.AccountPlans.Model;
using System.Collections.Generic;

namespace Doppler.AccountPlans.Dtos
{
    public class PromotionDto
    {
        public string Code { get; set; }
        public bool CanApply { get; set; }
        public bool ExpiredPromocode { get; set; }
        public Promotion PromotionApplied { get; set; }
        public IList<PlanPromotionDto> PlanPromotions { get; set; }
    }
}
