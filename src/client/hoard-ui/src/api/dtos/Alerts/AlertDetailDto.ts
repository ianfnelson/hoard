export interface AlertDetailDto {
  id: number
  createdUtc: string
  triggeredDate: string
  alertTypeId: number
  alertTypeName: string
  instrumentId: number | null
  instrumentName: string | null
  portfolioId: number | null
  portfolioName: string | null
  title: string
  message: string
  url: string | null
  urlTitle: string | null
}
