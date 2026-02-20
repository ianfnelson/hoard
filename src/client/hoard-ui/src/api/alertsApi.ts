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

export async function getAlerts(params?: GetAlertsParams): Promise<PagedResult<AlertSummaryDto>> {
  const response = await hoardApi.get('/alerts', { params })
  return response.data
}

export async function getAlertDetail(alertId: number): Promise<AlertDetailDto> {
  const response = await hoardApi.get(`/alerts/${alertId}`)
  return response.data
}
