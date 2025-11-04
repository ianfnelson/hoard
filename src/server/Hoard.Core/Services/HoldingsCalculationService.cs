using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Services
{
    public interface IHoldingsCalculationService
    {
        Task<int> CalculateHoldingsAsync(DateOnly asOfDate, CancellationToken ct = default);
    }
    
    public class HoldingsCalculationService : IHoldingsCalculationService
    {
        private readonly HoardContext _db;
        private readonly ILogger<HoldingsCalculationService> _logger;

        public HoldingsCalculationService(HoardContext db, ILogger<HoldingsCalculationService> logger)
        {
            _db = db;
            _logger = logger;
        }
        
        public async Task<int> CalculateHoldingsAsync(DateOnly asOfDate, CancellationToken ct = default)
        {
            _logger.LogInformation("Starting holdings calculation for {Date}", asOfDate.ToIsoDateString());

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                var parameters = new[]
                {
                    new SqlParameter("@AsOfDate", asOfDate.ToDateTime(TimeOnly.MinValue))
                };
                
                var result = await _db.Database.ExecuteSqlRawAsync("EXEC CalculateHoldings @AsOfDate", parameters, cancellationToken: ct);

                sw.Stop();

                _logger.LogInformation(
                    "Holdings calculated for {Date} ({Count} rows affected) in {Elapsed} ms",
                    asOfDate.ToIsoDateString(), result, sw.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate holdings for {Date}", asOfDate.ToIsoDateString());
                throw;
            }
        }
    }
}