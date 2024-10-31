using Doppler.AccountPlans.Enums;

namespace Doppler.AccountPlans.Model
{
    public class PlanInformation
    {
        public int EmailQty { get; set; }
        public decimal Fee { get; set; }
        public int SubscribersQty { get; set; }
        public string Type { get; set; }
        public int CurrentMonthPlan { get; set; }
        public UserTypesEnum IdUserType { get; set; }
        public int? DiscountPlanFeeAdmin { get; set; }
        public decimal? ChatPlanFee { get; set; }
        public int? ChatPlanConversationQty { get; set; }
        public int? PrintQty { get; set; }
    }
}
