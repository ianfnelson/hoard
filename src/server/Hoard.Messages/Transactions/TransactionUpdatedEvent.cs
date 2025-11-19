namespace Hoard.Messages.Transactions;

public record TransactionUpdatedEvent(Guid CorrelationId, int Id, DateOnly Date);