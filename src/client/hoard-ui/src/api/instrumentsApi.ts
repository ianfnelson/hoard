import { hoardApi } from './hoardApi'
import type { InstrumentSummaryDto } from './dtos/InstrumentSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult.ts'
import type { LookupDto } from './dtos/LookupDto'

export interface GetInstrumentsParams {
  instrumentTypeId?: number
  assetClassId?: number
  assetSubclassId?: number
  enablePriceUpdates?: boolean
  enableNewsUpdates?: boolean
  pageNumber?: number
  pageSize?: number
  search?: string
  sortBy?: string
  sortDirection?: string
}

export async function getInstruments(
  params?: GetInstrumentsParams
): Promise<PagedResult<InstrumentSummaryDto>> {
  const response = await hoardApi.get('/instruments', { params })
  return response.data
}

export async function getInstrumentsLookup(): Promise<LookupDto[]> {
  const response = await hoardApi.get('/instruments/lookup')
  return response.data
}
