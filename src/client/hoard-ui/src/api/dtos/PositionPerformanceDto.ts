export interface PositionPerformanceDto {
  costBasis: number
  units: number

  // Valuation snapshot
  value: number
  previousValue: number
  valueChange: number

  // Gains breakdown
  unrealisedGain: number
  realisedGain: number
  gain: number
  income: number
  profit: number

  // Absolute value changes
  valueChange1Y: number | null

  // Returns (nullable = insufficient history)
  return1D: number | null
  return1W: number | null
  return1M: number | null
  return3M: number | null
  return6M: number | null
  return1Y: number | null
  return3Y: number | null
  return5Y: number | null
  return10Y: number | null
  returnYtd: number | null
  returnAllTime: number | null
  annualisedReturn: number | null

  updatedUtc: string // ISO 8601 DateTime
}
