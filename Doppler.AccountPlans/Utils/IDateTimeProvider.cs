using System;

namespace Doppler.AccountPlans.Utils
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
