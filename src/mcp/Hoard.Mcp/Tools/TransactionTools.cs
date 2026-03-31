using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Hoard.Mcp.Tools;

[McpServerToolType]
public static class TransactionTools
{
    [McpServerTool, Description("Search and list transactions. Results are paged. Can filter by date range, account, instrument, or transaction type.")]
    public static async Task<string> GetTransactions(
        HoardApiClient api,
        [Description("Filter from date in yyyy-MM-dd format")] string? fromDate,
        [Description("Filter to date in yyyy-MM-dd format")] string? toDate,
        [Description("Filter by account ID")] int? accountId,
        [Description("Filter by instrument ID")] int? instrumentId,
        [Description("Filter by transaction type ID (use GetTransactionTypes for valid IDs)")] int? transactionTypeId,
        [Description("Search text")] string? search,
        [Description("Page number (default 1)")] int? pageNumber,
        [Description("Page size (default 10)")] int? pageSize,
        CancellationToken ct) =>
        await api.GetAsync("api/transactions" + HoardApiClient.BuildQuery(
            ("FromDate", fromDate), ("ToDate", toDate), ("AccountId", accountId),
            ("InstrumentId", instrumentId), ("TransactionTypeId", transactionTypeId),
            ("Search", search), ("PageNumber", pageNumber), ("PageSize", pageSize)), ct);

    [McpServerTool, Description("Get detailed information about a specific transaction, including charges and fees breakdown.")]
    public static async Task<string> GetTransaction(
        HoardApiClient api,
        [Description("The transaction ID")] int id,
        CancellationToken ct) =>
        await api.GetAsync($"api/transactions/{id}", ct);

    [McpServerTool, Description("Create a new transaction. Use GetTransactionTypes to find valid transaction type IDs. Use GetInstrumentLookup to find instrument IDs.")]
    public static async Task<string> CreateTransaction(
        HoardApiClient api,
        [Description("The account ID")] int accountId,
        [Description("The instrument ID (nullable for cash-only transactions like deposits)")] int? instrumentId,
        [Description("The transaction type ID (use GetTransactionTypes for valid IDs)")] int transactionTypeId,
        [Description("Transaction date in yyyy-MM-dd format")] string date,
        [Description("Number of units/shares")] double? units,
        [Description("Price per unit")] double? price,
        [Description("Total transaction value")] double? value,
        [Description("Optional notes")] string? notes,
        [Description("Contract note reference")] string? contractNoteReference,
        [Description("Dealing charge amount")] double? dealingCharge,
        [Description("FX charge amount")] double? fxCharge,
        [Description("Stamp duty amount")] double? stampDuty,
        [Description("PTM levy amount")] double? ptmLevy,
        CancellationToken ct) =>
        await api.PostAsync("api/transactions", new
        {
            accountId,
            instrumentId,
            transactionTypeId,
            date,
            units,
            price,
            value,
            notes,
            contractNoteReference,
            dealingCharge,
            fxCharge,
            stampDuty,
            ptmLevy
        }, ct);

    [McpServerTool, Description("Update an existing transaction. All fields are replaced — pass the complete transaction data.")]
    public static async Task<string> UpdateTransaction(
        HoardApiClient api,
        [Description("The transaction ID to update")] int id,
        [Description("The account ID")] int accountId,
        [Description("The instrument ID (nullable for cash-only transactions)")] int? instrumentId,
        [Description("The transaction type ID")] int transactionTypeId,
        [Description("Transaction date in yyyy-MM-dd format")] string date,
        [Description("Number of units/shares")] double? units,
        [Description("Price per unit")] double? price,
        [Description("Total transaction value")] double? value,
        [Description("Optional notes")] string? notes,
        [Description("Contract note reference")] string? contractNoteReference,
        [Description("Dealing charge amount")] double? dealingCharge,
        [Description("FX charge amount")] double? fxCharge,
        [Description("Stamp duty amount")] double? stampDuty,
        [Description("PTM levy amount")] double? ptmLevy,
        CancellationToken ct) =>
        await api.PutAsync($"api/transactions/{id}", new
        {
            accountId,
            instrumentId,
            transactionTypeId,
            date,
            units,
            price,
            value,
            notes,
            contractNoteReference,
            dealingCharge,
            fxCharge,
            stampDuty,
            ptmLevy
        }, ct);

    [McpServerTool, Description("Delete a transaction by ID. This will trigger recalculation of holdings and valuations.")]
    public static async Task<string> DeleteTransaction(
        HoardApiClient api,
        [Description("The transaction ID to delete")] int id,
        CancellationToken ct) =>
        await api.DeleteAsync($"api/transactions/{id}", ct);
}
