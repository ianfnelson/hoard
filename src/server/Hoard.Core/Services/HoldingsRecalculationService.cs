using Hoard.Core.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Services
{
    public interface IHoldingsRecalculationService
    {
        Task<int> RecalculateHoldingsAsync(DateOnly asOfDate, int? accountId = null, CancellationToken ct = default);
    }
    
    public class HoldingsRecalculationService : IHoldingsRecalculationService
    {
        private readonly HoardContext _db;
        private readonly ILogger<HoldingsRecalculationService> _logger;

        public HoldingsRecalculationService(HoardContext db, ILogger<HoldingsRecalculationService> logger)
        {
            _db = db;
            _logger = logger;
        }
        
        public async Task<int> RecalculateHoldingsAsync(DateOnly asOfDate, int? accountId = null, CancellationToken ct = default)
        {
            _logger.LogInformation("Starting holdings recalculation for {Date}, {Account}", asOfDate.ToString("yyyy-MM-dd"), 
                accountId.HasValue ? accountId.Value : "all accounts");

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                var sql = accountId.HasValue
                    ? "EXEC RecalculateHoldings @AsOfDate, @AccountId"
                    : "EXEC RecalculateHoldings @AsOfDate";
                
                var parameters = new[]
                {
                    new SqlParameter("@AsOfDate", asOfDate.ToDateTime(TimeOnly.MinValue)),
                    new SqlParameter("@AccountId", accountId ?? (object)DBNull.Value)
                };
                
                var result = await _db.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken: ct);

                sw.Stop();

                _logger.LogInformation(
                    "Holdings recalculated for {Date}, {Account} ({Count} rows affected) in {Elapsed} ms",
                    asOfDate.ToString("yyyy-MM-dd"), accountId.HasValue ? accountId.Value : "all accounts", result, sw.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recalculate holdings for {Date}", asOfDate.ToString("yyyy-MM-dd"));
                throw;
            }
        }
    }
}