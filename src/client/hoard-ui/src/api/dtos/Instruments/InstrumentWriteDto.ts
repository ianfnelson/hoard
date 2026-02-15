import type { InstrumentDataFeedWriteDto } from './InstrumentDataFeedWriteDto'

export interface InstrumentWriteDto {
  name: string | null
  instrumentTypeId: number | null
  assetSubclassId: number | null
  currencyId: string | null
  tickerDisplay: string | null
  isin: string | null
  dataFeeds?: InstrumentDataFeedWriteDto[] | null
}
