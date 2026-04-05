import { hoardApi } from './hoardApi'
import type { PortfolioDetailDto } from './dtos/Portfolios/PortfolioDetailDto'
import type { PortfolioPositionsDto } from './dtos/Portfolios/PortfolioPositionsDto'
import type { PortfolioExposureDto } from './dtos/Portfolios/PortfolioExposureDto'
import type { PortfolioInstrumentTypesDto } from './dtos/Portfolios/PortfolioInstrumentTypesDto'
import type { PortfolioSummaryDto } from './dtos/Portfolios/PortfolioSummaryDto'
import type { PortfolioSnapshotsDto } from './dtos/Portfolios/PortfolioSnapshotsDto'
import type { PortfolioValuationsDto } from './dtos/Portfolios/PortfolioValuationsDto'
import type { PortfolioValuationSeriesDto } from './dtos/Portfolios/PortfolioValuationSeriesDto'

export interface GetPortfolioPositionsParams {
  isOpen?: boolean
}

export interface GetPortfolioValuationsParams {
  from?: string
  to?: string
}

export type DisaggregateBy =
  | 'none'
  | 'accounts'
  | 'asset-classes'
  | 'asset-subclasses'
  | 'instrument-types'

export interface GetPortfolioValuationSeriesParams {
  from?: string
  to?: string
  disaggregateBy?: DisaggregateBy
}

export async function getPortfolioList(): Promise<PortfolioSummaryDto[]> {
  const response = await hoardApi.get('/portfolios')
  return response.data
}

export async function getPortfolioDetail(portfolioId: number): Promise<PortfolioDetailDto> {
  const response = await hoardApi.get(`/portfolios/${portfolioId}`)
  return response.data
}

export async function getPortfolioPositions(
  portfolioId: number,
  params?: GetPortfolioPositionsParams
): Promise<PortfolioPositionsDto> {
  const response = await hoardApi.get(`/portfolios/${portfolioId}/positions`, {
    params,
  })
  return response.data
}

export async function getPortfolioExposure(portfolioId: number): Promise<PortfolioExposureDto> {
  const response = await hoardApi.get(`/portfolios/${portfolioId}/exposure`)
  return response.data
}

export async function getPortfolioInstrumentTypes(
  portfolioId: number
): Promise<PortfolioInstrumentTypesDto> {
  const response = await hoardApi.get(`/portfolios/${portfolioId}/instrument-types`)
  return response.data
}

export async function getPortfolioSnapshots(
  portfolioId: number,
  signal?: AbortSignal
): Promise<PortfolioSnapshotsDto> {
  const response = await hoardApi.get(`/portfolios/${portfolioId}/snapshots`, { signal })
  return response.data
}

export async function getPortfolioValuations(
  portfolioId: number,
  params?: GetPortfolioValuationsParams
): Promise<PortfolioValuationsDto> {
  const response = await hoardApi.get(`/portfolios/${portfolioId}/valuation`, {
    params,
  })
  return response.data
}

export async function downloadPortfolioValuationsPdf(
  portfolioId: number,
  params?: GetPortfolioValuationsParams
) {
  const response = await hoardApi.get(`/portfolios/${portfolioId}/valuation/export`, {
    params,
    responseType: 'blob',
  })
  return response
}

export async function getPortfolioValuationSeries(
  portfolioId: number,
  params?: GetPortfolioValuationSeriesParams,
  signal?: AbortSignal
): Promise<PortfolioValuationSeriesDto> {
  const response = await hoardApi.get(`/portfolios/${portfolioId}/valuation-series`, {
    params,
    signal,
  })
  return response.data
}
