export interface PriceWriteDto {
  instrumentId: number | null
  asOfDate: string | null
  source: string | null
  isLocked: boolean | null
  open: number | null
  high: number | null
  low: number | null
  close: number | null
  volume: number | null
  adjustedClose: number | null
}
