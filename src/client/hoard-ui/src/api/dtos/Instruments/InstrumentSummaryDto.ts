export interface InstrumentSummaryDto {
  id: number
  tickerDisplay: string
  name: string
  instrumentTypeName: string
  assetClassName: string
  assetSubclassName: string
  enablePriceUpdates: boolean
  enableNewsUpdates: boolean
}
