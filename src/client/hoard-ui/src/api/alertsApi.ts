import { hoardApi } from './hoardApi'
import type { AlertSummaryDto } from './dtos/Alerts/AlertSummaryDto'
import type { AlertDetailDto } from './dtos/Alerts/AlertDetailDto'
import type { PagedResult } from '@/api/dtos/PagedResult'

export interface GetAlertsParams {
  alertTypeId?: number
  instrumentId?: number
  portfolioId?: number
  search?: string
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: string
}

export async function getAlerts(
  params?: GetAlertsParams,
  signal?: AbortSignal
): Promise<PagedResult<AlertSummaryDto>> {
  const response = await hoardApi.get('/alerts', { params, signal })
  return response.data
}

export async function getAlertDetail(
  alertId: number,
  signal?: AbortSignal
): Promise<AlertDetailDto> {
  const response = await hoardApi.get(`/alerts/${alertId}`, { signal })
  return response.data
}
