export interface PortfolioStatementDto {
  year: number

  // Performance metrics
  startValue: number
  endValue: number
  valueChange: number
  averageValue: number
  return: number
  churn: number
  yield: number

  // Trading metrics
  totalBuys: number
  totalSells: number
  countTrades: number

  // Income
  totalIncomeInterest: number
  totalIncomeLoyaltyBonus: number
  totalIncomeDividends: number
  totalPromotion: number

  // Deposits
  totalDepositPersonal: number
  totalDepositEmployer: number
  totalDepositIncomeTaxReclaim: number
  totalDepositTransferIn: number

  // Withdrawals & Fees
  totalWithdrawals: number
  totalFees: number
  totalDealingCharge: number
  totalStampDuty: number
  totalPtmLevy: number
  totalFxCharge: number

  updatedUtc: string
}
