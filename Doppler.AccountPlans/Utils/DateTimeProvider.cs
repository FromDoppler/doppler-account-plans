using System;

namespace Doppler.AccountPlans.Utils
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
