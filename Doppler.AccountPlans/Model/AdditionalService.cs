namespace Doppler.AccountPlans.Model
{
    public class AdditionalService
    {
        public int IdAddOnType { get; set; }
        public decimal Fee { get; set; }
        public int Qty { get; set; }
        public int? PromotionId { get; set; }
        public int? AddOnPromotionDiscount { get; set; }
        public int? AddOnPromotionDuration { get; set; }
    }
}
