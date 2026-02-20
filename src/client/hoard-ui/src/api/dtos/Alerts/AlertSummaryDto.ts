export interface AlertSummaryDto {
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
}
