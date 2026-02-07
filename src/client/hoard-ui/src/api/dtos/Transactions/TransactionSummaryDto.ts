export interface TransactionSummaryDto {
  id: number
  accountId: number
  accountName: string
  contractNoteReference: string
  date: string // ISO date: YYYY-MM-DD
  instrumentId?: number
  instrumentTicker?: string
  instrumentName?: string
  transactionTypeId: number
  transactionTypeName: string
  units?: number
  value: number
  price?: number
}
