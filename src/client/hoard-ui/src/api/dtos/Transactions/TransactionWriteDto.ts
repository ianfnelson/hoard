export interface TransactionWriteDto {
  accountId: number | null
  instrumentId: number | null
  transactionTypeId: number | null
  date: string | null // ISO: YYYY-MM-DD
  notes: string | null
  units: number | null
  contractNoteReference: string | null
  dealingCharge: number | null
  fxCharge: number | null
  stampDuty: number | null
  ptmLevy: number | null
  price: number | null
  value: number | null
}
