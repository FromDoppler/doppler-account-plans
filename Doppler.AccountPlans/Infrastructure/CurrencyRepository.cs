using Dapper;
using Doppler.AccountPlans.Model;
using System;
using System.Threading.Tasks;

namespace Doppler.AccountPlans.Infrastructure
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly IDatabaseConnectionFactory connectionFactory;

        public CurrencyRepository(IDatabaseConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<CurrencyRate> GetCurrencyRate(int idCurrencyTypeFrom, int idCurrencyTypeTo, DateTime date)
        {
            using var connection = connectionFactory.GetConnection();

            var currencyRate = await connection.QueryFirstOrDefaultAsync<CurrencyRate>(@"
SELECT IdCurrencyRate, IdCurrencyTypeFrom, IdCurrencyTypeTo, Rate, UTCFromDate, Active
FROM [CurrencyRate] WITH(NOLOCK)
WHERE ((IdCurrencyTypeFrom = @idCurrencyTypeFrom AND IdCurrencyTypeTo = @idCurrencyTypeTo) OR
(IdCurrencyTypeFrom = @idCurrencyTypeTo AND IdCurrencyTypeTo = @idCurrencyTypeFrom)) AND
UTCFromDate <= @date
ORDER BY UTCFromDate DESC",
                new
                {
                    date = DateTime.UtcNow,
                    idCurrencyTypeFrom,
                    idCurrencyTypeTo,
                });

            return currencyRate;
        }
    }
}
