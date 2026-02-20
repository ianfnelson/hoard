export interface CandleDto {
  x: number
  y: [number, number, number, number]
}

export interface PriceSeriesDto {
  candles: CandleDto[]
}
