export interface PortfolioValuationItemDto {
  instrumentId: number
  instrumentName: string
  toValue: number | null
  toPercentage: number | null
  fromValue: number | null
  fromPercentage: number | null
  changeValue: number | null
  changePercentage: number | null
}
