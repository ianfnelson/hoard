export interface InstrumentDataFeedWriteDto {
  dataFeedId: number | null
  enabled: boolean | null
  ticker: string | null
  importStartUtc?: string | null
}
