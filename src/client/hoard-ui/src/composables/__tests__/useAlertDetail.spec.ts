import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useAlertDetail } from '../useAlertDetail'
import * as alertsApi from '@/api/alertsApi'
import type { AlertDetailDto } from '@/api/dtos/Alerts/AlertDetailDto'

vi.mock('@/api/alertsApi')

describe('useAlertDetail', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { alert, isLoading, error } = useAlertDetail()

    expect(alert.value).toBeNull()
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch alert successfully', async () => {
    const mockAlert = { id: 1, message: 'Price dropped' } as AlertDetailDto
    vi.mocked(alertsApi.getAlertDetail).mockResolvedValue(mockAlert)

    const { alert, fetchAlert } = useAlertDetail()
    await fetchAlert(1)

    expect(alert.value).toEqual(mockAlert)
    expect(alertsApi.getAlertDetail).toHaveBeenCalledWith(1)
  })

  it('should handle error and clear alert', async () => {
    vi.mocked(alertsApi.getAlertDetail).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { alert, error, fetchAlert } = useAlertDetail()
    await fetchAlert(99)

    expect(alert.value).toBeNull()
    expect(error.value).toBe('Failed to load alert')
  })
})
