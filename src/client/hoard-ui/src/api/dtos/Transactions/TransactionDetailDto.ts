export interface TransactionDetailDto {
  id: number
  accountId: number
  accountName: string
  contractNoteReference?: string
  date: string // ISO date: YYYY-MM-DD
  instrumentId?: number
  instrumentTicker?: string
  instrumentName?: string
  notes?: string
  transactionTypeId: number
  transactionTypeName: string
  units?: number
  price?: number
  value: number
  dealingCharge?: number
  stampDuty?: number
  ptmLevy?: number
  fxCharge?: number
}
