using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages.Alerts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Hoard.Core.Application.Alerts;

public interface IAlertService
{
    Task<bool> TryRaiseAsync(AlertRequest request, CancellationToken ct = default);
}

public sealed class AlertService(HoardContext db, IBus bus) : IAlertService
{
    public async Task<bool> TryRaiseAsync(
        AlertRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request.Title, nameof(request.Title));
        ArgumentNullException.ThrowIfNull(request.Message, nameof(request.Message));

        var alert = new Alert
        {
            AlertTypeId = request.AlertTypeId,
            TriggeredDate = request.TriggeredDate,
            CreatedUtc = DateTime.UtcNow,
            InstrumentId = request.InstrumentId,
            PortfolioId = request.PortfolioId,
            SuppressionKey = request.SuppressionKey,
            Title = request.Title,
            Message = request.Message,
            Url = request.Url,
            UrlTitle = request.UrlTitle
        };

        db.Alerts.Add(alert);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Suppressed due to duplicate suppression key
            return false;
        }

        var alertRaisedEvent = new AlertRaisedEvent(
            alert.AlertTypeId, alert.TriggeredDate, 
            alert.CreatedUtc, alert.Title, alert.Message,
            alert.Url, alert.UrlTitle);
        
        await bus.Publish(alertRaisedEvent);

        return true;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sql)
        {
            // 2601 = duplicate key
            // 2627 = unique constraint violation
            return sql.Number is 2601 or 2627;
        }

        return false;
    }
}