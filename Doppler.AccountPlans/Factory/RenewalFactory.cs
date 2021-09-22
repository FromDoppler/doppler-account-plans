using System;
using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.RenewalHandlers;

namespace Doppler.AccountPlans.Factory
{
    public class RenewalFactory : IRenewalFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public RenewalFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public RenewalHandler CreateHandler(int renewalType)
        {
            return renewalType switch
            {
                (int)RenewalPeriodEnum.Monthly => (MonthlyHandler)_serviceProvider.GetService(typeof(MonthlyHandler)),
                (int)RenewalPeriodEnum.Quarterly => (QuarterlyHandler)_serviceProvider.GetService(typeof(QuarterlyHandler)),
                (int)RenewalPeriodEnum.Biannual => (BiannualHandler)_serviceProvider.GetService(typeof(BiannualHandler)),
                (int)RenewalPeriodEnum.Annual => (AnnualHandler)_serviceProvider.GetService(typeof(AnnualHandler)),
                _ => null
            };
        }
    }
}
