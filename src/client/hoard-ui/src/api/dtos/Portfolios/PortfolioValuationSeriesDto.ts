export interface PortfolioValuationPointDto {
  x: number
  y: number
}

export interface PortfolioValuationSeriesGroupDto {
  name: string
  color: string | null
  data: PortfolioValuationPointDto[]
}

export interface PortfolioValuationSeriesDto {
  series: PortfolioValuationSeriesGroupDto[]
}
