export interface ValuationPointDto {
  x: number
  y: number
}

export interface ValuationSeriesAccountDto {
  name: string
  color: string | null
  data: ValuationPointDto[]
}

export interface ValuationSeriesDto {
  series: ValuationSeriesAccountDto[]
}
