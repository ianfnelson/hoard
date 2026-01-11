using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountType",
                columns: table => new
                {
                    AccountTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountType", x => x.AccountTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AssetClass",
                columns: table => new
                {
                    AssetClassId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetClass", x => x.AssetClassId);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    CurrencyId = table.Column<string>(type: "char(3)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.CurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentType",
                columns: table => new
                {
                    InstrumentTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCash = table.Column<bool>(type: "bit", nullable: false),
                    IsFxPair = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentType", x => x.InstrumentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Portfolio",
                columns: table => new
                {
                    PortfolioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolio", x => x.PortfolioId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionType",
                columns: table => new
                {
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionType", x => x.TransactionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountTypeId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Account_AccountType_AccountTypeId",
                        column: x => x.AccountTypeId,
                        principalTable: "AccountType",
                        principalColumn: "AccountTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetSubclass",
                columns: table => new
                {
                    AssetSubclassId = table.Column<int>(type: "int", nullable: false),
                    AssetClassId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetSubclass", x => x.AssetSubclassId);
                    table.ForeignKey(
                        name: "FK_AssetSubclass_AssetClass_AssetClassId",
                        column: x => x.AssetClassId,
                        principalTable: "AssetClass",
                        principalColumn: "AssetClassId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioPerformance",
                columns: table => new
                {
                    PortfolioPerformanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    CashValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Income = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Return1D = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1W = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return3M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return6M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return3Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return5Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return10Y = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReturnYtd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReturnAllTime = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AnnualisedReturn = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioPerformance", x => x.PortfolioPerformanceId);
                    table.ForeignKey(
                        name: "FK_PortfolioPerformance_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioSnapshot",
                columns: table => new
                {
                    PortfolioSnapshotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StartValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Return = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Churn = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalBuys = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSells = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncomeInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncomeLoyaltyBonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncomePromotion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIncomeDividends = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalFees = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDealingCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalStampDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPtmLevy = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalFxCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositPersonal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositEmployer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositIncomeTaxReclaim = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositTransferIn = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalWithdrawals = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CountTrades = table.Column<int>(type: "int", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioSnapshot", x => x.PortfolioSnapshotId);
                    table.ForeignKey(
                        name: "FK_PortfolioSnapshot_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioValuation",
                columns: table => new
                {
                    PortfolioValuationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioValuation", x => x.PortfolioValuationId);
                    table.ForeignKey(
                        name: "FK_PortfolioValuation_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioAccount",
                columns: table => new
                {
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioAccount", x => new { x.PortfolioId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_PortfolioAccount_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioAccount_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Instrument",
                columns: table => new
                {
                    InstrumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstrumentTypeId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AssetSubclassId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CurrencyId = table.Column<string>(type: "char(3)", nullable: false),
                    TickerApi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ticker = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Isin = table.Column<string>(type: "char(12)", nullable: true),
                    EnablePriceUpdates = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrument", x => x.InstrumentId);
                    table.ForeignKey(
                        name: "FK_Instrument_AssetSubclass_AssetSubclassId",
                        column: x => x.AssetSubclassId,
                        principalTable: "AssetSubclass",
                        principalColumn: "AssetSubclassId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrument_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currency",
                        principalColumn: "CurrencyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrument_InstrumentType_InstrumentTypeId",
                        column: x => x.InstrumentTypeId,
                        principalTable: "InstrumentType",
                        principalColumn: "InstrumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TargetAllocation",
                columns: table => new
                {
                    TargetAllocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    AssetSubclassId = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetAllocation", x => x.TargetAllocationId);
                    table.CheckConstraint("CK_Target_0_100", "[Target] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_TargetAllocation_AssetSubclass_AssetSubclassId",
                        column: x => x.AssetSubclassId,
                        principalTable: "AssetSubclass",
                        principalColumn: "AssetSubclassId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TargetAllocation_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Holding",
                columns: table => new
                {
                    HoldingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holding", x => x.HoldingId);
                    table.ForeignKey(
                        name: "FK_Holding_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Holding_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Position",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    OpenDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CloseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_Position_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Position_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Price",
                columns: table => new
                {
                    PriceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RetrievedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    High = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Low = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Close = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: true),
                    AdjustedClose = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Price", x => x.PriceId);
                    table.ForeignKey(
                        name: "FK_Price_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quote",
                columns: table => new
                {
                    QuoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    RetrievedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bid = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Ask = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FiftyTwoWeekHigh = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FiftyTwoWeekLow = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RegularMarketPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RegularMarketChange = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RegularMarketChangePercent = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quote", x => x.QuoteId);
                    table.ForeignKey(
                        name: "FK_Quote_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: true),
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractNoteReference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Units = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DealingCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StampDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PtmLevy = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FxCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transaction_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaction_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_TransactionType_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionType",
                        principalColumn: "TransactionTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoldingValuation",
                columns: table => new
                {
                    HoldingValuationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoldingId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoldingValuation", x => x.HoldingValuationId);
                    table.ForeignKey(
                        name: "FK_HoldingValuation_Holding_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "Holding",
                        principalColumn: "HoldingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PositionPerformance",
                columns: table => new
                {
                    PositionPerformanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    CostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Income = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Return1D = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1W = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return3M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return6M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return3Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return5Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return10Y = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReturnYtd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReturnAllTime = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AnnualisedReturn = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionPerformance", x => x.PositionPerformanceId);
                    table.ForeignKey(
                        name: "FK_PositionPerformance_Position_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Position",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_AccountTypeId",
                table: "Account",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetClass_Code",
                table: "AssetClass",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetSubclass_AssetClassId",
                table: "AssetSubclass",
                column: "AssetClassId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetSubclass_Code",
                table: "AssetSubclass",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holding_AccountId_InstrumentId_AsOfDate",
                table: "Holding",
                columns: new[] { "AccountId", "InstrumentId", "AsOfDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holding_InstrumentId",
                table: "Holding",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_HoldingValuation_HoldingId",
                table: "HoldingValuation",
                column: "HoldingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_AssetSubclassId",
                table: "Instrument",
                column: "AssetSubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_CurrencyId",
                table: "Instrument",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_InstrumentTypeId",
                table: "Instrument",
                column: "InstrumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_Isin",
                table: "Instrument",
                column: "Isin",
                unique: true,
                filter: "[Isin] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentType_Code",
                table: "InstrumentType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioAccount_AccountId",
                table: "PortfolioAccount",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioPerformance_PortfolioId",
                table: "PortfolioPerformance",
                column: "PortfolioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSnapshot_PortfolioId",
                table: "PortfolioSnapshot",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioValuation_PortfolioId_AsOfDate",
                table: "PortfolioValuation",
                columns: new[] { "PortfolioId", "AsOfDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Position_InstrumentId",
                table: "Position",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Position_PortfolioId",
                table: "Position",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionPerformance_PositionId",
                table: "PositionPerformance",
                column: "PositionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Price_InstrumentId_AsOfDate",
                table: "Price",
                columns: new[] { "InstrumentId", "AsOfDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quote_InstrumentId",
                table: "Quote",
                column: "InstrumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TargetAllocation_AssetSubclassId",
                table: "TargetAllocation",
                column: "AssetSubclassId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetAllocation_PortfolioId_AssetSubclassId",
                table: "TargetAllocation",
                columns: new[] { "PortfolioId", "AssetSubclassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountId",
                table: "Transaction",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Date",
                table: "Transaction",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_InstrumentId",
                table: "Transaction",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionTypeId",
                table: "Transaction",
                column: "TransactionTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoldingValuation");

            migrationBuilder.DropTable(
                name: "PortfolioAccount");

            migrationBuilder.DropTable(
                name: "PortfolioPerformance");

            migrationBuilder.DropTable(
                name: "PortfolioSnapshot");

            migrationBuilder.DropTable(
                name: "PortfolioValuation");

            migrationBuilder.DropTable(
                name: "PositionPerformance");

            migrationBuilder.DropTable(
                name: "Price");

            migrationBuilder.DropTable(
                name: "Quote");

            migrationBuilder.DropTable(
                name: "TargetAllocation");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Holding");

            migrationBuilder.DropTable(
                name: "Position");

            migrationBuilder.DropTable(
                name: "TransactionType");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Instrument");

            migrationBuilder.DropTable(
                name: "Portfolio");

            migrationBuilder.DropTable(
                name: "AccountType");

            migrationBuilder.DropTable(
                name: "AssetSubclass");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "InstrumentType");

            migrationBuilder.DropTable(
                name: "AssetClass");
        }
    }
}
