using Doppler.AccountPlans.Enums;
using System;
using System.Collections.Generic;

namespace Doppler.AccountPlans.Model
{
    public class UserPlan
    {
        public int IdUser { get; set; }
        public int EmailQty { get; set; }
        public decimal Fee { get; set; }
        public int SubscribersQty { get; set; }
        public string Type { get; set; }
        public int CurrentMonthPlan { get; set; }
        public UserTypesEnum IdUserType { get; set; }
        public int? DiscountPlanFeeAdmin { get; set; }
        public int? DiscountPlanFeePromotion { get; set; }
        public string PromotionCode { get; set; }
        public int IdUserTypePlan { get; set; }
        public DateTime Date { get; set; }
        public int TotalMonthPlan { get; set; }
        public int IdDiscountPlan { get; set; }
        public IList<AdditionalService> AdditionalServices { get; set; }
    }
}
