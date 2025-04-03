namespace Doppler.AccountPlans.Model
{
    public class PushNotificationPlanInformation : BasePlanInformation
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Fee { get; set; }
        public int? FreeDays { get; set; }
        public decimal Additional { get; set; }
    }
}
