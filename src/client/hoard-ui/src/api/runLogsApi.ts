import { hoardApi } from './hoardApi'
import type { RunLogDto } from './dtos/System/RunLogDto'
import type { PagedResult } from './dtos/PagedResult'

export interface GetRunLogsParams {
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: string
  runTypeId?: number
}

export async function getRunLogs(params?: GetRunLogsParams): Promise<PagedResult<RunLogDto>> {
  const response = await hoardApi.get<PagedResult<RunLogDto>>('/run-logs', { params })
  return response.data
}
