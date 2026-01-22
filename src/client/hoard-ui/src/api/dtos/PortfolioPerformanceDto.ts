export interface PortfolioPerformanceDto {
  // Snapshot values
  value: number;
  cashValue: number;
  cashPercentage: number;
  previousValue: number;
  valueChange: number;

  // Gains
  unrealisedGain: number;
  realisedGain: number;
  income: number;

  // Absolute value changes
  valueChange1Y: number | null;

  // Returns (nullable = not enough data yet)
  return1D: number | null;
  return1W: number | null;
  return1M: number | null;
  return3M: number | null;
  return6M: number | null;
  return1Y: number | null;
  return3Y: number | null;
  return5Y: number | null;
  return10Y: number | null;
  returnYtd: number | null;
  returnAllTime: number | null;
  annualisedReturn: number | null;

  updatedUtc: string; // ISO string
}
