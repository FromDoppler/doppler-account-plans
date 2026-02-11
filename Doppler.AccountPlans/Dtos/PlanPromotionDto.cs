using Doppler.AccountPlans.Enums;

namespace Doppler.AccountPlans.Dtos
{
    public class PlanPromotionDto
    {
        public UserTypesEnum? PlanType { get; set; }
        public string Quantity { get; set; }
    }
}
