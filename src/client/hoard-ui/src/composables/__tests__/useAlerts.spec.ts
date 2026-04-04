import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useAlerts } from '../useAlerts'
import * as alertsApi from '@/api/alertsApi'
import type { AlertSummaryDto } from '@/api/dtos/Alerts/AlertSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult'

vi.mock('@/api/alertsApi')

describe('useAlerts', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { items, totalCount, isLoading, error } = useAlerts()

    expect(items.value).toEqual([])
    expect(totalCount.value).toBe(0)
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch alerts successfully', async () => {
    const mockResponse: PagedResult<AlertSummaryDto> = {
      items: [
        {
          id: 1,
          createdUtc: '2026-01-01T00:00:00Z',
          triggeredDate: '2026-01-01',
          alertTypeId: 1,
          alertTypeName: 'Price',
          instrumentId: null,
          instrumentName: null,
          portfolioId: null,
          portfolioName: null,
          title: 'Test alert',
        } satisfies AlertSummaryDto,
      ],
      totalCount: 1,
    }
    vi.mocked(alertsApi.getAlerts).mockResolvedValue(mockResponse)

    const { items, totalCount, fetchAlerts } = useAlerts()
    await fetchAlerts({ pageSize: 10 })

    expect(items.value).toEqual(mockResponse.items)
    expect(totalCount.value).toBe(1)
    expect(alertsApi.getAlerts).toHaveBeenCalledWith({ pageSize: 10 })
  })

  it('should handle error', async () => {
    vi.mocked(alertsApi.getAlerts).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { items, totalCount, error, fetchAlerts } = useAlerts()
    await fetchAlerts()

    expect(items.value).toEqual([])
    expect(totalCount.value).toBe(0)
    expect(error.value).toBe('Failed to load alerts')
  })
})
