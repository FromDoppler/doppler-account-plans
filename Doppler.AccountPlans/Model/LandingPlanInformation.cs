namespace Doppler.AccountPlans.Model
{
    public class LandingPlanInformation : BasePlanInformation
    {
        public string Description { get; set; }
        public int LandingsQty { get; set; }
        public decimal Fee { get; set; }
    }
}
