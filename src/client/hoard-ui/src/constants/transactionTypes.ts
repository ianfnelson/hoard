export const TransactionTypeIds = {
  Buy: 100,
  Sell: 200,
  CorporateAction: 300,
  DepositPersonal: 401,
  DepositEmployer: 402,
  DepositTransfer: 403,
  DepositIncomeTaxReclaim: 404,
  Withdrawal: 500,
  Fee: 600,
  IncomeInterest: 701,
  IncomeLoyaltyBonus: 702,
  IncomeDividend: 704,
  Promotion: 800,
} as const

// Types requiring instrument field
export const INSTRUMENT_REQUIRED_TYPES = [100, 200, 300, 704] // Buy, Sell, CorporateAction, Dividend

// Types allowing optional instrument
export const INSTRUMENT_OPTIONAL_TYPES = [702] // LoyaltyBonus

// Types showing trading fields (units, price, fees)
export const TRADING_FIELD_TYPES = [100, 200, 300] // Buy, Sell, CorporateAction
