import { hoardApi } from './hoardApi'
import type { InstrumentDetailDto } from './dtos/Instruments/InstrumentDetailDto'
import type { InstrumentSummaryDto } from './dtos/Instruments/InstrumentSummaryDto'
import type { PriceSummaryDto } from './dtos/Instruments/PriceSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult'
import type { LookupDto } from './dtos/LookupDto.ts'

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

export async function getInstrumentDetail(id: number): Promise<InstrumentDetailDto> {
  const response = await hoardApi.get(`/instruments/${id}`)
  return response.data
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

export interface GetPricesParams {
  pageNumber?: number
  pageSize?: number
}

export async function getPrices(
  instrumentId: number,
  params?: GetPricesParams
): Promise<PagedResult<PriceSummaryDto>> {
  const response = await hoardApi.get(`/instruments/${instrumentId}/prices`, { params })
  return response.data
}
