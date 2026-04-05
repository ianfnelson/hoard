import { hoardApi } from './hoardApi'
import type { InstrumentDetailDto } from './dtos/Instruments/InstrumentDetailDto'
import type { InstrumentSummaryDto } from './dtos/Instruments/InstrumentSummaryDto'
import type { InstrumentWriteDto } from './dtos/Instruments/InstrumentWriteDto'
import type { PriceSummaryDto } from './dtos/Instruments/PriceSummaryDto'
import type { PriceSeriesDto } from './dtos/Instruments/PriceSeriesDto'
import type { ValuationSeriesDto } from './dtos/Instruments/ValuationSeriesDto'
import type { PagedResult } from '@/api/dtos/PagedResult'
import type { LookupDto } from './dtos/LookupDto'

export interface GetInstrumentsParams {
  instrumentTypeId?: number
  assetClassId?: number
  assetSubclassId?: number
  pageNumber?: number
  pageSize?: number
  search?: string
  sortBy?: string
  sortDirection?: string
}

export async function getInstrumentDetail(
  id: number,
  signal?: AbortSignal
): Promise<InstrumentDetailDto> {
  const response = await hoardApi.get(`/instruments/${id}`, { signal })
  return response.data
}

export async function getInstruments(
  params?: GetInstrumentsParams,
  signal?: AbortSignal
): Promise<PagedResult<InstrumentSummaryDto>> {
  const response = await hoardApi.get('/instruments', { params, signal })
  return response.data
}

export async function getInstrumentsLookup(): Promise<LookupDto[]> {
  const response = await hoardApi.get('/instruments/lookup')
  return response.data
}

export async function createInstrument(data: InstrumentWriteDto): Promise<number> {
  const response = await hoardApi.post('/instruments', data)
  return response.data
}

export async function updateInstrument(id: number, data: InstrumentWriteDto): Promise<void> {
  await hoardApi.put(`/instruments/${id}`, data)
}

export async function deleteInstrument(id: number): Promise<void> {
  await hoardApi.delete(`/instruments/${id}`)
}

export interface GetPriceSeriesParams {
  from: string
  to: string
}

export async function getPriceSeries(
  instrumentId: number,
  from: string,
  to: string,
  signal?: AbortSignal
): Promise<PriceSeriesDto> {
  const response = await hoardApi.get(`/instruments/${instrumentId}/price-series`, {
    params: { from, to },
    signal,
  })
  return response.data
}

export async function getValuationSeries(
  instrumentId: number,
  signal?: AbortSignal
): Promise<ValuationSeriesDto> {
  const response = await hoardApi.get(`/instruments/${instrumentId}/valuation-series`, { signal })
  return response.data
}

export interface GetPricesParams {
  pageNumber?: number
  pageSize?: number
}

export async function getPrices(
  instrumentId: number,
  params?: GetPricesParams,
  signal?: AbortSignal
): Promise<PagedResult<PriceSummaryDto>> {
  const response = await hoardApi.get(`/instruments/${instrumentId}/prices`, { params, signal })
  return response.data
}
