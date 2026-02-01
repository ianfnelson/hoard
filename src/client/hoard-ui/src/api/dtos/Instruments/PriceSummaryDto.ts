export interface PriceSummaryDto {
  id: number
  asOfDate: string
  open: number | null
  high: number | null
  low: number | null
  close: number
  volume: number | null
  adjustedClose: number
}
