import type { InstrumentQuoteDto } from './InstrumentQuoteDto'
import type { InstrumentDataFeedDto } from './InstrumentDataFeedDto'

export interface InstrumentDetailDto {
  id: number
  name: string
  tickerDisplay: string
  isin: string | null
  createdUtc: string
  instrumentTypeId: number
  instrumentTypeName: string
  assetClassId: number
  assetClassName: string
  assetSubclassId: number
  assetSubclassName: string
  currencyId: string
  currencyName: string
  quote: InstrumentQuoteDto | null
  dataFeeds: InstrumentDataFeedDto[]
}
