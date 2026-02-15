export interface PriceDetailDto {
  id: number
  instrumentId: number
  instrumentName: string
  instrumentTicker: string
  asOfDate: string
  retrievedUtc: string
  source: string
  isLocked: boolean
  open: number | null
  high: number | null
  low: number | null
  close: number
  volume: number | null
  adjustedClose: number
}
