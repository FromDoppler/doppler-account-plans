namespace Doppler.AccountPlans.Model
{
    public class OnSitePlanInformation : BasePlanInformation
    {
        public string Description { get; set; }
        public int PrintQty { get; set; }
        public decimal Fee { get; set; }
        public decimal AdditionalPrint { get; set; }
    }
}
