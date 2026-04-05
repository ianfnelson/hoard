import { hoardApi } from './hoardApi'
import type { InstrumentTypeDto } from './dtos/InstrumentTypes/InstrumentTypeDto'
import type { AssetClassDto } from './dtos/AssetClasses/AssetClassDto'
import type { TransactionTypeDto } from './dtos/TransactionTypes/TransactionTypeDto'
import type { CurrencyDto } from './dtos/Currencies/CurrencyDto'
import type { AlertTypeDto } from './dtos/AlertTypes/AlertTypeDto'
import type { RunTypeDto } from './dtos/RunLogTypes/RunTypeDto'

export async function getInstrumentTypes(signal?: AbortSignal): Promise<InstrumentTypeDto[]> {
  const response = await hoardApi.get('/reference/instrument-types/', { signal })
  return response.data
}

export async function getAssetClasses(signal?: AbortSignal): Promise<AssetClassDto[]> {
  const response = await hoardApi.get('/reference/asset-classes/', { signal })
  return response.data
}

export async function getTransactionTypes(signal?: AbortSignal): Promise<TransactionTypeDto[]> {
  const response = await hoardApi.get('/reference/transaction-types/', { signal })
  return response.data
}

export async function getCurrencies(signal?: AbortSignal): Promise<CurrencyDto[]> {
  const response = await hoardApi.get('/reference/currencies/', { signal })
  return response.data
}

export async function getAlertTypes(signal?: AbortSignal): Promise<AlertTypeDto[]> {
  const response = await hoardApi.get('/reference/alert-types/', { signal })
  return response.data
}

export async function getRunLogTypes(signal?: AbortSignal): Promise<RunTypeDto[]> {
  const response = await hoardApi.get('/reference/run-log-types/', { signal })
  return response.data
}
