namespace Doppler.AccountPlans.Model
{
    public class ConversationPlanInformation : BasePlanInformation
    {
        public decimal Fee { get; set; }
        public int ConversationsQty { get; set; }
        public int Agents { get; set; }
        public int Channels { get; set; }
        public decimal AdditionalConversation { get; set; }
        public decimal AdditionalAgent { get; set; }
        public decimal AdditionalChannel { get; set; }
    }
}
