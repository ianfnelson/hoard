export interface PortfolioValuationItemDto {
  instrumentId: number
  instrumentName: string
  instrumentTicker: string
  toUnits: number | null
  toValue: number | null
  toPercentage: number | null
  fromUnits: number | null
  fromValue: number | null
  fromPercentage: number | null
  changeValue: number | null
  changePercentage: number | null
}
