import type { InstrumentDataFeedWriteDto } from './InstrumentDataFeedWriteDto'

export interface InstrumentWriteDto {
  name: string | null
  instrumentTypeId: number | null
  assetSubclassId: number | null
  currencyId: string | null
  ticker: string | null
  isin: string | null
  dataFeeds?: InstrumentDataFeedWriteDto[] | null
}
