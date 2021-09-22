namespace Doppler.AccountPlans.Model
{
    public class PlanInformation
    {
        public int EmailQty { get; set; }
        public decimal Fee { get; set; }
        public int SubscribersQty { get; set; }
        public string Type { get; set; }
        public int CurrentMonthPlan { get; set; }
    }
}
