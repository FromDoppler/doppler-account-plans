using Doppler.AccountPlans.Model;
using System;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public interface ICurrencyRepository
    {
        Task<CurrencyRate> GetCurrencyRate(int idCurrencyTypeFrom, int idCurrencyTypeTo, DateTime date);
    }
}
