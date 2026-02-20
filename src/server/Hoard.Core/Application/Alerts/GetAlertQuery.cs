using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Alerts;

public record GetAlertQuery(int AlertId) : IQuery<AlertDetailDto?>;

public sealed class GetAlertHandler(HoardContext context, ILogger<GetAlertHandler> logger)
    : IQueryHandler<GetAlertQuery, AlertDetailDto?>
{
    public async Task<AlertDetailDto?> HandleAsync(GetAlertQuery query, CancellationToken ct = default)
    {
        var dto = await context.Alerts
            .AsNoTracking()
            .Where(a => a.Id == query.AlertId)
            .Select(a => new AlertDetailDto
            {
                Id = a.Id,
                CreatedUtc = a.CreatedUtc,
                TriggeredDate = a.TriggeredDate,
                AlertTypeId = a.AlertTypeId,
                AlertTypeName = a.AlertType.Name,
                InstrumentId = a.InstrumentId,
                InstrumentName = a.Instrument == null ? null : a.Instrument.Name,
                PortfolioId = a.PortfolioId,
                PortfolioName = a.Portfolio == null ? null : a.Portfolio.Name,
                Title = a.Title,
                Message = a.Message,
                Url = a.Url,
                UrlTitle = a.UrlTitle
            }).SingleOrDefaultAsync(ct);

        if (dto == null)
        {
            logger.LogWarning(
                "Alert with id {AlertId} not found",
                query.AlertId);
        }

        return dto;
    }
}
