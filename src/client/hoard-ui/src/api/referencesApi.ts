import { hoardApi } from './hoardApi'
import type { InstrumentTypeDto } from './dtos/InstrumentTypes/InstrumentTypeDto'
import type { AssetClassDto } from './dtos/AssetClasses/AssetClassDto'
import type { TransactionTypeDto } from './dtos/TransactionTypes/TransactionTypeDto'

export async function getInstrumentTypes(): Promise<InstrumentTypeDto[]> {
  const response = await hoardApi.get('/reference/instrument-types/')
  return response.data
}

export async function getAssetClasses(): Promise<AssetClassDto[]> {
  const response = await hoardApi.get('/reference/asset-classes/')
  return response.data
}

export async function getTransactionTypes(): Promise<TransactionTypeDto[]> {
  const response = await hoardApi.get('/reference/transaction-types/')
  return response.data
}
