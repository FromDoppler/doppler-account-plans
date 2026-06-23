namespace Doppler.AccountPlans.Model
{
    public class BillingInformation
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public int PaymentMethod { get; set; }
    }
}
